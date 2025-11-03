using BeanScene.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();


builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

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

app.Run();
