using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebProject.Data;
using WebProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebProject.Controllers;

[Authorize(Roles = "admin")]
public class CategoryController : Controller
{
    private readonly AppDbContext _context;

    public CategoryController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var categories = _context.Categories.ToList();
        return View(categories);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
        return View();
    }

    [HttpPost]
    public IActionResult Create(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
        return View(category);
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var category = _context.Categories.Find(id);
        if (category == null)
            return NotFound();

        ViewBag.Categories = new SelectList(
            _context.Categories.Where(c => c.Id != id), "Id", "Name", category.ParentCategoryId);

        return View(category);
    }

    [HttpPost]
    public IActionResult Edit(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Categories.Update(category);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        ViewBag.Categories = new SelectList(
            _context.Categories.Where(c => c.Id != category.Id), "Id", "Name", category.ParentCategoryId);

        return View(category);
    }

    public IActionResult Delete(int id)
    {
        var category = _context.Categories
            .Include(c => c.SubCategories)
            .FirstOrDefault(c => c.Id == id);

        if (category == null)
            return NotFound();

        if (category.SubCategories != null && category.SubCategories.Any())
        {
            TempData["Error"] = "Bu kategorinin alt kategorileri var, silinemez!";
            return RedirectToAction("Index");
        }

        _context.Categories.Remove(category);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }



    public JsonResult GetChildCategories(int parentId)
    {
        var children = _context.Categories
            .Where(c => c.ParentCategoryId == parentId)
            .Select(c => new { c.Id, c.Name })
            .ToList();

        return Json(children);
    }
}
