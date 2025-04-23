using Microsoft.AspNetCore.Mvc;
using WebProject.Models;
using WebProject.ViewModels;

namespace WebProject.Controllers;
public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Kay�t i�lemleri (ileride eklenecek)
            return RedirectToAction("Login");
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Giri� i�lemleri (ileride eklenecek)
            return RedirectToAction("Index", "Home");
        }
        return View(model);
    }
}
