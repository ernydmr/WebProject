using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProject.Data;
using WebProject.Models; // Eðer `User` gibi modeller kullanýyorsan
using WebProject.ViewModels; // Register/Login ViewModel’lerini kullanmak için

namespace WebProject.Controllers;
public class ProductController : Controller
{

    private readonly AppDbContext _context;

    public ProductController(AppDbContext context)
    {
        _context = context;
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
    public IActionResult AddComment(Comment comment)
    {
        if (ModelState.IsValid)
        {
            comment.CreatedAt = DateTime.Now;
            _context.Comments.Add(comment);
            _context.SaveChanges();
        }

        return RedirectToAction("Detail", new { id = comment.ProductId });
    }
}
