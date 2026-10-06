using Microsoft.AspNetCore.Identity;

namespace WineShop.Utility
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync(WC.AdminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(WC.AdminRole));
            }

            if (!await roleManager.RoleExistsAsync(WC.CustomerRole))
            {
                await roleManager.CreateAsync(new IdentityRole(WC.CustomerRole));
            }
        }
    }
}