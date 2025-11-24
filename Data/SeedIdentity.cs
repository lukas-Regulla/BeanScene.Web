using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace BeanScene.Web.Data
{
    public static class SeedIdentity
    {
        public static async Task EnsureSeededAsync(IServiceProvider services)
        {
            var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();
            UserManager<ApplicationUser> userMgr = services.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = new[] { "Admin", "Staff", "Member" };

            foreach (var r in roles)
            {
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole(r));
            }

            // Admin user
            const string adminEmail = "admin@beanscene.com";
            const string adminPass = "Admin!234"; // change after first login

            var existing = await userMgr.FindByEmailAsync(adminEmail);
            if (existing == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var create = await userMgr.CreateAsync(admin, adminPass);
                if (create.Succeeded)
                {
                    await userMgr.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}
