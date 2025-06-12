using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebProject.Data;
using WebProject.Models;
using WebProject.ViewModels;

namespace WebProject.Controllers;

[Authorize]
public class OrderController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrderController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Checkout(int id)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null || product.Stock <= 0)
            {
                TempData["Error"] = "Ürün stokta bulunmuyor.";
                return RedirectToAction("Detail", "Product", new { id = id });
            }

            var addresses = await _context.Addresses
                .Where(a => a.UserId == user.Id)
                .ToListAsync();

            if (!addresses.Any())
            {
                TempData["Error"] = "Sipariş verebilmek için önce bir adres eklemelisiniz.";
                return RedirectToAction("Add", "Address");
            }

            var viewModel = new OrderViewModel
            {
                ProductId = id,
                Addresses = addresses,
                Quantity = 1
            };

            ViewBag.Product = product;
            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Bir hata oluştu: " + ex.Message;
            return RedirectToAction("Detail", "Product", new { id = id });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(OrderViewModel model)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Oturum süreniz dolmuş. Lütfen tekrar giriş yapın." });
            }

            // Adresleri yükle
            model.Addresses = await _context.Addresses
                .Where(a => a.UserId == user.Id)
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var selectedProduct = await _context.Products.FindAsync(model.ProductId);
            if (selectedProduct == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı." });
            }

            var address = await _context.Addresses.FindAsync(model.SelectedAddressId);

            if (selectedProduct.Stock < model.Quantity)
            {
                return Json(new { success = false, message = "Ürün stokta yeterli miktarda bulunmuyor." });
            }

            if (address == null || address.UserId != user.Id)
            {
                return Json(new { success = false, message = "Geçersiz adres seçimi." });
            }

            if (user.Balance < (selectedProduct.Price * model.Quantity))
            {
                return Json(new { success = false, message = "Bakiyeniz yetersiz." });
            }

            var order = new Order
            {
                UserId = user.Id,
                AddressId = model.SelectedAddressId,
                TotalPrice = selectedProduct.Price * model.Quantity,
                Items = new List<OrderItem>
                {
                    new OrderItem 
                    { 
                        ProductId = selectedProduct.Id,
                        Quantity = model.Quantity
                    }
                }
            };

            user.Balance -= (selectedProduct.Price * model.Quantity);
            selectedProduct.Stock -= model.Quantity;

            _context.Orders.Add(order);
            _context.Products.Update(selectedProduct);
            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                redirectUrl = Url.Action("OrderHistory"),
                message = "Siparişiniz başarıyla oluşturuldu."
            });
        }
        catch (Exception ex)
        {
            return Json(new { 
                success = false, 
                message = "Sipariş oluşturulurken bir hata oluştu: " + ex.Message 
            });
        }
    }

    [Authorize]
    public async Task<IActionResult> OrderHistory()
    {
        var userId = _userManager.GetUserId(User);
        var orders = await _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Address)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }
}
