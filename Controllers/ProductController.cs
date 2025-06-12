using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using WebProject.Data;
using WebProject.Models; // E�er `User` gibi modeller kullan�yorsan
using WebProject.ViewModels;
using System.Threading.Tasks; // Register/Login ViewModel�lerini kullanmak i�in

namespace WebProject.Controllers;
public class ProductController : Controller
{

    private readonly AppDbContext _context;

    private readonly UserManager<ApplicationUser> _userManager;

    public ProductController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        var products = _context.Products
            .Include( p => p.User)
            .ToList(); // 
        return View(products);
    }

    public IActionResult Detail(int id)
    {
        var product = _context.Products
            .Include(p => p.User)
            .FirstOrDefault(p => p.Id == id);
        if (product == null) return NotFound();

        var comments = _context.Comments
            .Where(c => c.ProductId == id)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        var viewModel = new ProductDetailViewModel
        {
            Product = product,
            Comments = comments,
            NewComment = new Comment { ProductId = id }
        };

        return View("Detail", viewModel);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddComment(Comment comment)
    {
       
        var user = await _userManager.GetUserAsync(User);
        comment.UserName = user?.UserName ?? "Anonim";

        ModelState.Remove("UserName");
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Detail", new { id = comment.ProductId });
        }

        comment.CreatedAt = DateTime.Now;
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return RedirectToAction("Detail", new { id = comment.ProductId });
    }

    [AllowAnonymous]
    public async Task<IActionResult> SellerProducts(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return NotFound();

        var seller = await _userManager.FindByIdAsync(userId);
        if (seller == null)
            return NotFound();

        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.User)
            .Where(p => p.UserId == userId)
            .ToListAsync();

        ViewBag.Seller = seller;
        return View("Index", products);
    }



    [Authorize]
    public async Task<IActionResult> Buy(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null || product.Stock <= 0)
        {
            TempData["Error"] = "Ürün Stokta Yok!";
            return RedirectToAction("Detail", new { id = id });
        }

        // OrderController'daki Checkout action'ına yönlendir
        return RedirectToAction("Checkout", "Order", new { id = id });
    }
}
