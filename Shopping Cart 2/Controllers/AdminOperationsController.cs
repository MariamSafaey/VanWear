using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Cart_2.Constants;
using Shopping_Cart_2.Models;
using Shopping_Cart_2.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping_Cart_2.Controllers
{
    [Authorize(Roles = nameof(Roles.Admin))]
    public class AdminOperationsController : Controller
    {
        private readonly IUserOrderService _userOrderService;
        private readonly IManageItemService _manageItemService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminOperationsController(
            IUserOrderService userOrderService,
            IManageItemService manageItemService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userOrderService = userOrderService;
            _manageItemService = manageItemService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> CreateFirstAdmin()
        {
            try
            {
                if (!await _roleManager.RoleExistsAsync(nameof(Roles.Admin)))
                {
                    await _roleManager.CreateAsync(new IdentityRole(nameof(Roles.Admin)));
                }

                var adminEmail = "initialadmin@example.com";
                var adminUser = await _userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        City = "Assuit",
                        Name = "Initial Admin",
                        ProfilePic = "pic",
                        Street = "feryal",
                        PhoneNumber = "0123456789",
                        EmailConfirmed = true,
                        DateOfJoin = DateTime.Now
                    };
                    var result = await _userManager.CreateAsync(adminUser, "TempPassword123!");
                    if (!result.Succeeded)
                    {
                        return BadRequest($"User creation failed: {string.Join(", ", result.Errors)}");
                    }
                }

                if (!await _userManager.IsInRoleAsync(adminUser, nameof(Roles.Admin)))
                {
                    await _userManager.AddToRoleAsync(adminUser, nameof(Roles.Admin));
                }

                return Content($"Successfully created initial admin: {adminEmail}");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // User Management
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new List<UserWithRolesViewModel>();

            foreach (var user in users)
            {
                userRoles.Add(new UserWithRolesViewModel
                {
                    User = user,
                    Roles = await _userManager.GetRolesAsync(user)
                });
            }

            return View(userRoles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var userToDelete = await _userManager.FindByIdAsync(id);

            if (userToDelete == null)
            {
                TempData["ErrorMessage"] = "User not found";
                return RedirectToAction(nameof(ManageUsers));
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (userToDelete.Id == currentUser.Id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account!";
                return RedirectToAction(nameof(ManageUsers));
            }

            var result = await _userManager.DeleteAsync(userToDelete);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User deleted successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting user: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdminRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found";
                return RedirectToAction(nameof(ManageUsers));
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (user.Id == currentUser.Id)
            {
                TempData["ErrorMessage"] = "You cannot modify your own admin role!";
                return RedirectToAction(nameof(ManageUsers));
            }

            if (await _userManager.IsInRoleAsync(user, nameof(Roles.Admin)))
            {
                await _userManager.RemoveFromRoleAsync(user, nameof(Roles.Admin));
                TempData["SuccessMessage"] = "Admin role removed successfully";
            }
            else
            {
                await _userManager.AddToRoleAsync(user, nameof(Roles.Admin));
                TempData["SuccessMessage"] = "Admin role added successfully";
            }

            return RedirectToAction(nameof(ManageUsers));
        }

        // Original Order Management Methods
        public async Task<IActionResult> AllOrders()
        {
            var orders = await _userOrderService.AllOrders();
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateOrderStatus(int orderId)
        {
            var order = await _userOrderService.GetOrderById(orderId);
            if (order == null)
            {
                throw new InvalidOperationException($"Order with id:{orderId} does not found.");
            }
            var orderStatusList = _userOrderService.GetSelectLists();
            var data = new UpdateOrderStatusModel
            {
                OrderId = orderId,
                OrderStatusId = order.OrderStatusId,
                OrderStatusList = orderStatusList
            };
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(UpdateOrderStatusModel data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    data.OrderStatusList = _userOrderService.GetSelectLists();
                    return View(data);
                }
                await _userOrderService.ChangeOrderStatus(data);
                TempData["msg"] = "Updated successfully";
            }
            catch
            {
                TempData["msg"] = "Something went wrong";
            }
            return RedirectToAction(nameof(UpdateOrderStatus), new { orderId = data.OrderId });
        }

        public async Task<IActionResult> TogglePaymentStatus(int orderId)
        {
            try
            {
                await _userOrderService.TogglePaymentStatus(orderId);
            }
            catch (Exception ex)
            {
                // log exception here
            }
            return RedirectToAction(nameof(AllOrders));
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public async Task<IActionResult> ToggleApprovementStatus(int ItemId)
        {
            try
            {
                await _manageItemService.ToggleApprovementStatus(ItemId);
            }
            catch (Exception ex)
            {
                // log exception here
            }
            return RedirectToAction(nameof(GetAllItems));
        }

        public async Task<IActionResult> GetAllItems()
        {
            var items = await _manageItemService.GetAllItems();
            return View(items);
        }
    }

    public class UserWithRolesViewModel
    {
        public ApplicationUser User { get; set; }
        public IList<string> Roles { get; set; }
    }

}