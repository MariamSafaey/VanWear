using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopping_Cart_2.Models;
using Shopping_Cart_2.Models.DTO;
using Shopping_Cart_2.Services;
using System.Threading.Tasks;

namespace Shopping_Cart_2.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IActionResult> AddItem(int itemId, int qty = 1, int redirect = 0)
        {
            try
            {
                var cartCount = await _cartService.AddItem(itemId, qty);
                if (redirect == 0)
                    return Ok(cartCount);
                return RedirectToAction(nameof(GetUserCart));
            }
            catch
            {
                // You could log error here
                return BadRequest("Could not add item to cart.");
            }
        }

        public async Task<IActionResult> RemoveItem(int itemId)
        {
            try
            {
                var cartCount = await _cartService.RemoveItem(itemId);
                return RedirectToAction(nameof(GetUserCart));
            }
            catch
            {
                // You could log error here
                return BadRequest("Could not remove item from cart.");
            }
        }

        public async Task<IActionResult> GetUserCart()
        {
            var cart = await _cartService.GetUserCart();
            return View(cart);
        }

        // Called via script to display cart count in layout
        public async Task<IActionResult> GetTotalItemInCart()
        {
            int cartItem = await _cartService.GetCartItemCount();
            return Ok(cartItem);
        }

        public IActionResult Checkout()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                bool isCheckedOut = await _cartService.DoCheckout(model);
                if (!isCheckedOut)
                    return RedirectToAction(nameof(OrderFailure));
                return RedirectToAction(nameof(OrderSuccess));
            }
            catch
            {
                // You could log error here
                return RedirectToAction(nameof(OrderFailure));
            }
        }

        public IActionResult OrderSuccess()
        {
            return View();
        }

        public IActionResult OrderFailure()
        {
            return View();
        }
    }
}
