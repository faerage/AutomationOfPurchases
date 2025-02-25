using AutomationOfPurchases.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutomationOfPurchases.API.Infrastructure.Seed
{
    public static class DatabaseSeeder
    {
        /// <summary>
        /// Застосовує міграції і створює дефолтні ролі + тестових користувачів.
        /// </summary>
        public static async Task SeedTestDataAsync(
            AppDbContext db,
            RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager)
        {
            // 1) Застосовуємо міграції, якщо треба
            db.Database.Migrate();

            // 2) Створюємо ролі
            var roles = new[] { "User", "DepartmentHead", "Economist", "WarehouseWorker" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 3) Створюємо тестових користувачів (по одному на кожну роль)
            await CreateTestUserIfNotExists(userManager, "testUser", "User", "User123!", "Regular User");
            await CreateTestUserIfNotExists(userManager, "testDept", "DepartmentHead", "Dept123!", "Department Head");
            await CreateTestUserIfNotExists(userManager, "testEco", "Economist", "Eco123!", "Economist Person");
            await CreateTestUserIfNotExists(userManager, "testWh", "WarehouseWorker", "Wh123!", "Warehouse Worker");
        }

        private static async Task CreateTestUserIfNotExists(
            UserManager<AppUser> userManager,
            string userName,
            string roleName,
            string password,
            string fullName)
        {
            var existingUser = await userManager.FindByNameAsync(userName);
            if (existingUser == null)
            {
                var user = new AppUser
                {
                    UserName = userName,
                    Email = userName + "@test.com",
                    FullName = fullName
                };

                var createResult = await userManager.CreateAsync(user, password);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, roleName);
                    Console.WriteLine($"Test user '{userName}' created with role '{roleName}'.");
                }
                else
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    Console.WriteLine($"Failed to create user '{userName}': {errors}");
                }
            }
            else
            {
                Console.WriteLine($"Test user '{userName}' already exists.");
            }
        }
    }
}
