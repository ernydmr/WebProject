using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProject.Data;
using WebProject.Models;

namespace WebProject.Controllers;

[Authorize]
public class AddressController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AddressController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var addresses = await _context.Addresses
            .Where(a => a.UserId == user.Id).ToListAsync();

        return View(addresses);
    }

    [HttpGet]
    public IActionResult Add()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add(Address address)
    {
        try
        {
            // Önce kullanıcıyı al
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // User ve UserId alanlarını doldur
            address.UserId = user.Id;
            address.User = user;

            // ModelState'i temizle ve yeniden validasyon yap
            ModelState.Clear();
            TryValidateModel(address);

            // Model validasyonunu kontrol et
            if (!ModelState.IsValid)
            {
                return View(address);
            }

            // Adresi kaydet
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            // Başarılı ise Index sayfasına yönlendir
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            // Hata durumunda kullanıcıya bilgi ver
            ModelState.AddModelError("", "Adres kaydedilirken bir hata oluştu: " + ex.Message);
            return View(address);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var address = await _context.Addresses.FindAsync(id);
        if (address != null)
        {
            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }
}
