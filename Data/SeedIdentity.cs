using BeanScene.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            var demoAccounts = new[]
            {
                (Email: "admin@beanscene.com",  Password: "Admin123!",  Role: "Admin"),
                (Email: "staff@beanscene.com",  Password: "Staff123!",  Role: "Staff"),
                (Email: "member@beanscene.com", Password: "Member123!", Role: "Member"),
            };

            foreach (var (email, password, role) in demoAccounts)
            {
                var user = await userMgr.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
                    await userMgr.CreateAsync(user, password);
                    await userMgr.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
