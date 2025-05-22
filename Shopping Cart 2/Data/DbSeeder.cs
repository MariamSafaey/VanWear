using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shopping_Cart_2.Constants;
using Shopping_Cart_2.Models;

namespace Shopping_Cart_2.Data
{
    public static class DbSeeder
    {
        public static async Task SeedDefaultData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
                // Apply pending migrations
                await context.Database.MigrateAsync();
                logger.LogInformation("Applied pending migrations");

                // Seed Roles
                await SeedRolesAsync(roleManager, logger);

                //// Seed Admin User
                //await SeedAdminUserAsync(userManager, logger);

                // Seed Order Statuses
                await SeedOrderStatusesAsync(context, logger);

                logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
                logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            foreach (var role in Enum.GetNames(typeof(Roles)))
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    logger.LogInformation("Seeded {Role} role", role);
                }
            }
        }

        //private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, ILogger logger)
        //{
        //    const string adminEmail = "admin@gmail.com";
        //    const string adminPassword = "Admin@123";

        //    var adminUser = await userManager.FindByEmailAsync(adminEmail);
        //    if (adminUser == null)
        //    {
        //        adminUser = new ApplicationUser
        //        {
        //            UserName = adminEmail,
        //            Email = adminEmail,
        //            EmailConfirmed = true
        //        };

        //        var result = await userManager.CreateAsync(adminUser, adminPassword);
        //        if (result.Succeeded)
        //        {
        //            await userManager.AddToRoleAsync(adminUser, Roles.Admin.ToString());
        //            logger.LogInformation("Seeded admin user with email {Email}", adminEmail);
        //        }
        //        else
        //        {
        //            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        //            logger.LogError("Failed to create admin user: {Errors}", errors);
        //            throw new Exception($"Failed to create admin user: {errors}");
        //        }
        //    }
        //}

        private static async Task SeedOrderStatusesAsync(ApplicationDbContext context, ILogger logger)
        {
            if (!await context.orderStatuses.AnyAsync())
            {
                var statuses = new List<OrderStatus>
                {
                    new() { StatusName = "Pending" },
                    new() { StatusName = "Processing" },
                    new() { StatusName = "Shipped" },
                    new() { StatusName = "Delivered" },
                    new() { StatusName = "Cancelled" },
                    new() { StatusName = "Refunded" }
                };

                await context.orderStatuses.AddRangeAsync(statuses);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded order statuses");
            }
        }

        // Optional: Seed sample data for development
        public static async Task SeedSampleDataAsync(ApplicationDbContext context, ILogger logger)
        {
            if (!await context.items.AnyAsync() && !await context.categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new() { Name = "Women" },
                    new() { Name = "Men" },
                    new() { Name = "Kids" }
                };

                await context.categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();

                //var items = new List<Item>
                //{
                //    new()
                //    {
                //        Name = "Dress",
                //        Description = "Latest model Dress",
                //        Price = 599.99,
                //        CategoryId = categories[0].Id,
                //        Stock = new Stock { Quantity = 50 }
                //    },
                //    new()
                //    {
                //        Name = "Jeans",
                //        Description = "Guide to C# programming",
                //        Price = 39.99,
                //        CategoryId = categories[1].Id,
                //        Stock = new Stock { Quantity = 100 }
                //    },
                //    new()
                //    {
                //        Name = "T-Shirt",
                //        Description = "Cotton t-shirt",
                //        Price = 19.99,
                //        CategoryId = categories[2].Id,
                //        Stock = new Stock { Quantity = 200 }
                //    }
                //};

                await context.SaveChangesAsync();
                logger.LogInformation("Seeded sample categories and items");
            }
        }
    }
}