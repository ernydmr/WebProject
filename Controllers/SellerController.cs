using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebProject.Data;
using WebProject.Models;
using WebProject.ViewModels;

namespace WebProject.Controllers;

[Authorize(Roles = "seller")]
public class SellerController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public SellerController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> MyProducts()
    {
        var user = await _userManager.GetUserAsync(User);
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.UserId == user.Id)
            .ToListAsync();

        return View(products);
    }

    [HttpGet]
    public IActionResult AddProduct()
    {
        var viewModel = new ProductFormViewModel
        {
            TopCategories = _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct(ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.TopCategories = _context.Categories
                .Where(c => c.ParentCategoryId == null).ToList();
            return View(model);
        }

        string? imagePath = null;

        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            var uploadsFolder = Path.Combine("wwwroot", "images", "products");
            Directory.CreateDirectory(uploadsFolder);

            var fileExt = Path.GetExtension(model.ImageFile.FileName);
            var fileName = Guid.NewGuid().ToString() + fileExt;
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.ImageFile.CopyToAsync(stream);
            }

            imagePath = $"/images/products/{fileName}";
        }

        var product = new Product
        {
            Name = model.Name,
            Price = model.Price,
            Description = model.Description,
            Stock = model.Stock,
            CategoryId = model.CategoryId,
            ImageUrl = imagePath,
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return RedirectToAction("MyProducts");
    }

    [HttpGet]
    public async Task<IActionResult> EditProduct(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (product == null) return NotFound();

        var categories = _context.Categories.ToList();
        ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);

        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> EditProduct(Product updatedProduct, IFormFile? imageFile)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == updatedProduct.Id && p.UserId == userId);
        if (product == null) return NotFound();

        if (ModelState.IsValid)
        {
            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;
            product.Description = updatedProduct.Description;
            product.Stock = updatedProduct.Stock;
            product.CategoryId = updatedProduct.CategoryId;

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine("wwwroot", "images", "products");
                Directory.CreateDirectory(uploadsFolder);

                var ext = Path.GetExtension(imageFile.FileName);
                var fileName = Guid.NewGuid().ToString() + ext;
                var path = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                product.ImageUrl = $"/images/products/{fileName}";
            }

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyProducts");
        }

        var categories = _context.Categories.ToList();
        ViewBag.Categories = new SelectList(categories, "Id", "Name", updatedProduct.CategoryId);

        return View(updatedProduct);
    }

    public async Task<IActionResult> DeleteProduct(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("MyProducts");
    }
}
