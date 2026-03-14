using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WineShop.Data;

namespace WineShop.Utility
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            for (var i = 0; i < 10; i++)
            {
                try
                {
                    using var scope = services.CreateScope();

                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                    await db.Database.MigrateAsync();

                    if (!await roleManager.RoleExistsAsync(WC.AdminRole))
                    {
                        await roleManager.CreateAsync(new IdentityRole(WC.AdminRole));
                    }

                    if (!await roleManager.RoleExistsAsync(WC.CustomerRole))
                    {
                        await roleManager.CreateAsync(new IdentityRole(WC.CustomerRole));
                    }

                    break;
                }
                catch
                {
                    await Task.Delay(2000);
                }
            }
        }
    }
}
