using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebProject.Models; // Eðer `User` gibi modeller kullanýyorsan
using WebProject.ViewModels; // Register/Login ViewModel’lerini kullanmak için

namespace WebProject.Controllers;

[Authorize] // Giriþ yapýlmadan bu sayfa açýlmaz
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    public ProfileController(UserManager<ApplicationUser> userManager)
    {
        this._userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user);
        ViewBag.Roles = roles;
        return View(user);
    }
}
