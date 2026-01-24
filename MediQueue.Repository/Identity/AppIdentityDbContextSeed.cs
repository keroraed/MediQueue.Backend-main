using MediQueue.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace MediQueue.Repository.Identity;

public class AppIdentityDbContextSeed
{
    public static async Task SeedUsersAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Seed Roles
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        if (!await roleManager.RoleExistsAsync("Clinic"))
        {
            await roleManager.CreateAsync(new IdentityRole("Clinic"));
        }

        if (!await roleManager.RoleExistsAsync("Patient"))
        {
            await roleManager.CreateAsync(new IdentityRole("Patient"));
        }

        // Seed Admin User
        if (await userManager.FindByEmailAsync("admin@mediqueue.com") == null)
        {
            var adminUser = new AppUser
            {
                DisplayName = "System Administrator",
                Email = "admin@mediqueue.com",
                UserName = "admin@mediqueue.com",
                PhoneNumber = "+201012345678",
                DateCreated = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
