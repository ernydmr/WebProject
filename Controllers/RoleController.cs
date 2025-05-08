using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebProject.Models;

namespace WebProject.Controllers;

[Authorize(Roles = "admin")]
public class RoleController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        var roles = _roleManager.Roles.ToList();
        return View(roles);
    }

    [HttpPost]
    public async Task<IActionResult> Add(string roleName)
    {
        if (!string.IsNullOrWhiteSpace(roleName) && !await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName.Trim()));
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Rename(string id, string newName)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null || role.Name == "admin") return Forbid(); // admin ismi deðiþemez

        role.Name = newName.Trim();
        await _roleManager.UpdateAsync(role);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null || role.Name == "admin") return Forbid();

        // admin kullanýcýsý varsa rol silinemez
        var adminUser = await _userManager.FindByEmailAsync("admin@site.com");
        if (adminUser != null && await _userManager.IsInRoleAsync(adminUser, "admin"))
        {
            return Forbid();
        }

        await _roleManager.DeleteAsync(role);
        return RedirectToAction("Index");
    }
}
