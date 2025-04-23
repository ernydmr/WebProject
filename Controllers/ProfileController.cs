using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebProject.Models; // E�er `User` gibi modeller kullan�yorsan
using WebProject.ViewModels; // Register/Login ViewModel�lerini kullanmak i�in

namespace WebProject.Controllers;

[Authorize] // Giri� yap�lmadan bu sayfa a��lmaz
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
        return View(user);
    }
}
