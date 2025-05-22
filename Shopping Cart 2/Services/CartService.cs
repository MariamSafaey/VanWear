using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopping_Cart_2.Models;
using Shopping_Cart_2.Models.DTO;

namespace Shopping_Cart_2.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CartService> _logger;

        public CartService(
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            ILogger<CartService> logger)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }
            return _userManager.GetUserId(principal) ??
                   throw new UnauthorizedAccessException("User ID not found");
        }

        public async Task<ShoppingCart?> GetCart(string userId)
        {
            return await _db.ShoppingCarts
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }


        public async Task<int> AddItem(int itemId, int qty)
        {
            string userId = GetUserId();
            await using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var shoppingCart = await GetCart(userId) ?? new ShoppingCart
                {
                    UserId = userId
                };

                if (shoppingCart.Id == 0)
                {
                    await _db.ShoppingCarts.AddAsync(shoppingCart);
                    await _db.SaveChangesAsync();
                }

                var cartItem = await _db.CartDetails
                    .FirstOrDefaultAsync(a => a.ShoppingCartId == shoppingCart.Id && a.ItemId == itemId);

                if (cartItem != null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    var item = await _db.items.FindAsync(itemId) ??
                              throw new InvalidOperationException("Item not found");

                    cartItem = new CartDetail
                    {
                        ItemId = itemId,
                        ShoppingCartId = shoppingCart.Id,
                        Quantity = qty,
                        UnitPrice = item.Price
                    };
                    await _db.CartDetails.AddAsync(cartItem);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return await GetCartItemCount(userId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error adding item to cart");
                throw;
            }
        }

        public async Task<int> RemoveItem(int itemId)
        {
            string userId = GetUserId();

            try
            {
                var shoppingCart = await GetCart(userId);
                var cartItem = await _db.CartDetails
                    .FirstOrDefaultAsync(a => a.ShoppingCartId == shoppingCart.Id && a.ItemId == itemId)
                    ?? throw new InvalidOperationException("Item not found in cart");

                if (cartItem.Quantity == 1)
                {
                    _db.CartDetails.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity--;
                }

                await _db.SaveChangesAsync();
                return await GetCartItemCount(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart");
                throw;
            }
        }

        public async Task<ShoppingCart> GetUserCart()
        {
            var userId = GetUserId();

            var cart = await _db.ShoppingCarts
                .Include(a => a.CartDetails)
                    .ThenInclude(a => a.Item)
                        .ThenInclude(a => a.Stock)
                .Include(a => a.CartDetails)
                    .ThenInclude(a => a.Item)
                        .ThenInclude(a => a.Category)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (cart == null)
            {
                cart = new ShoppingCart { UserId = userId };
                _db.ShoppingCarts.Add(cart);
                await _db.SaveChangesAsync();
            }

            return cart;
        }


        public async Task<int> GetCartItemCount(string userId = "")
        {
            userId = string.IsNullOrEmpty(userId) ? GetUserId() : userId;
            var cart = await _db.ShoppingCarts
                .Include(x => x.CartDetails)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            return cart?.CartDetails.Sum(x => x.Quantity) ?? 0;
        }

        public async Task<bool> DoCheckout(CheckoutModel model)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var userId = GetUserId();
                var shoppingCart = await GetCart(userId);
                var cartDetails = await _db.CartDetails
                    .Where(a => a.ShoppingCartId == shoppingCart.Id)
                    .ToListAsync();

                if (cartDetails.Count == 0)
                {
                    throw new InvalidOperationException("Cart is empty");
                }

                // Stock validation
                foreach (var item in cartDetails)
                {
                    var stock = await _db.Stocks.FirstOrDefaultAsync(a => a.ItemId == item.ItemId)
                        ?? throw new InvalidOperationException($"Stock not found for item {item.ItemId}");

                    if (item.Quantity > stock.Quantity)
                    {
                        throw new InvalidOperationException($"Only {stock.Quantity} items available for item {item.ItemId}");
                    }
                }

                var pendingStatus = await _db.orderStatuses
                    .FirstOrDefaultAsync(s => s.StatusName == "Pending")
                    ?? new OrderStatus { StatusName = "Pending" };

                if (pendingStatus.Id == 0)
                {
                    await _db.orderStatuses.AddAsync(pendingStatus);
                    await _db.SaveChangesAsync();
                }

                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    OrderStatusId = pendingStatus.Id,
                    Name = model.Name,
                    Email = model.Email,
                    MobileNumber = model.MobileNumber,
                    PaymentMethod = model.PaymentMethod,
                    Address = model.Address,
                    IsPaid = false
                };

                await _db.Orders.AddAsync(order);
                await _db.SaveChangesAsync();

                foreach (var cartItem in cartDetails)
                {
                    var orderDetail = new OrderDetail
                    {
                        ItemId = cartItem.ItemId,
                        OrderId = order.Id,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.UnitPrice
                    };
                    await _db.OrderDetails.AddAsync(orderDetail);

                    var stock = await _db.Stocks.FirstOrDefaultAsync(a => a.ItemId == cartItem.ItemId);
                    stock!.Quantity -= cartItem.Quantity;
                }

                _db.CartDetails.RemoveRange(cartDetails);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error during checkout");
                throw;
            }
        }
    }
}