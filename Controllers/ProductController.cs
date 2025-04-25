using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using WebProject.Data;
using WebProject.Models; // Eðer `User` gibi modeller kullanýyorsan
using WebProject.ViewModels;
using System.Threading.Tasks; // Register/Login ViewModel’lerini kullanmak için

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
        var products = _context.Products.ToList(); // 
        return View(products);
    }

    public IActionResult Detail(int id)
    {
        var product = _context.Products.FirstOrDefault(p => p.Id == id);
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


    [Authorize]
    public async Task<IActionResult> Buy(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null || product.Stock <=0 )
        {
            return NotFound();
        }

        if (product.Stock <= 0)
        {
            TempData["Error"] = "Ürün Stokta Yok!";
            return RedirectToAction("Detail", new { id = id });
        }

        var UserId = _userManager.GetUserId(User);

        var order = new Order
        {
            UserId = UserId,
            CreatedAt = DateTime.Now,
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductId = product.Id
                }
            }
        };
        product.Stock--;
        _context.Orders.Add(order);
        _context.Products.Update(product);
        await _context.SaveChangesAsync();

        return RedirectToAction("OrderHistory", "Order");
    }
}
