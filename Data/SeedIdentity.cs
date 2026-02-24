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

        }
    }
}
