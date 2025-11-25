using System.Drawing;
using BeanScene.Web.Data;
using BeanScene.Web.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BeanScene.Web.Hubs;
using BeanScene.Web.Services;
using Microsoft.AspNetCore.Identity.UI.Services;



var builder = WebApplication.CreateBuilder(args);

// Connection string
var cs = builder.Configuration.GetConnectionString("DefaultConnection")
         ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Domain context (your app tables)
builder.Services.AddDbContext<BeanSceneContext>(opts => opts.UseSqlServer(cs));

// Identity context (AspNet* tables)
builder.Services.AddDbContext<ApplicationDbContext>(opts => opts.UseSqlServer(cs));

// *** Identity with Roles (guarantees RoleManager & stores) ***
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(o => {
        o.SignIn.RequireConfirmedAccount = true;
        o.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();


builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSignalR();



var app = builder.Build();

app.MapRazorComponents<BeanScene.Web.Components.ImageGenerator>()
    .AddInteractiveServerRenderMode();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Seed roles/admin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedIdentity.EnsureSeededAsync(services);
}

app.MapHub<ChatHub>("/chathub");

app.Run();
