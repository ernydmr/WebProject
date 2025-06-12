using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebProject.Models; // Eðer `User` gibi modeller kullanýyorsan
using WebProject.ViewModels; // Register/Login ViewModel’lerini kullanmak için
using WebProject.Data; // ApplicationDbContext için

namespace WebProject.Controllers;

[Authorize] // Giriþ yapýlmadan bu sayfa açýlmaz
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;
    public ProfileController(UserManager<ApplicationUser> userManager,AppDbContext context)
    {
        this._userManager = userManager;
        this._context = context;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user);
        var orders = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Where(o => o.UserId == user.Id)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        ViewBag.Orders = orders;
        ViewBag.Roles = roles;
        return View(user);
    }
}
