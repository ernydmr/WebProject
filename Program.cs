using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebProject.Data;
using WebProject.Models;
using WebProject.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ➤ Veritabanı bağlantısı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ➤ Identity + roller
builder.Services.AddDefaultIdentity<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// ➤ Yetkilendirme + SignalR
builder.Services.AddAuthorization();
builder.Services.AddSignalR();

// ➤ Kullanıcı ID'sini SignalR için tanımak üzere (NameIdentifier claim'i ekleniyor)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnValidatePrincipal = async context =>
    {
        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(context.Principal);
        if (user != null && context.Principal != null)
        {
            var identity = context.Principal.Identities.First();
            if (!identity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
            {
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
            }
        }
    };
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// ➤ Rol oluşturma ve admin/seller atama
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    if (!await roleManager.RoleExistsAsync("admin"))
        await roleManager.CreateAsync(new IdentityRole("admin"));

    if (!await roleManager.RoleExistsAsync("seller"))
        await roleManager.CreateAsync(new IdentityRole("seller"));

    var adminUser = await userManager.FindByEmailAsync("admin@site.com");
    if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "admin"))
        await userManager.AddToRoleAsync(adminUser, "admin");
}

// ➤ Middleware pipeline
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

// ➤ SignalR route
app.MapHub<MessageHub>("/messageHub");

// ➤ MVC routing
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
