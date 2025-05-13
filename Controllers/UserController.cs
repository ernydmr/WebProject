using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebProject.Data;
using WebProject.Models;
using WebProject.Services;



namespace WebProject.Controllers;

public class UserController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UserPresenceService _presence;


    public UserController(AppDbContext context,UserManager<ApplicationUser> userManager,UserPresenceService presence)
    {
        _context = context;
        _userManager = userManager;
        _presence = presence;
    }
    public async Task<IActionResult> Profile(string username) {
        if (string.IsNullOrEmpty(username)) return NotFound();

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == username);
        if (user == null) return NotFound();

        ViewBag.IsOnline = _presence.IsOnline(user.Id);
        return View(user);
    }
}