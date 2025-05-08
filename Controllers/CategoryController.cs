using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebProject.Data;
using WebProject.Models;
using Microsoft.AspNetCore.Authorization;
using WebProject.ViewModels;

namespace WebProject.Controllers;

[Authorize(Roles = "admin,seller")]
public class CategoryController : Controller
{
    private readonly AppDbContext _context;

    public CategoryController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var categories = _context.Categories
            .Include(c => c.ParentCategory)
            .ToList();

        ViewBag.HierarchicalList = GetHierarchicalCategoryList();
        return View(categories);
    }


    [HttpGet]
    public IActionResult Create()
    {
        var vm = new CategoryCreateViewModel
        {
            ParentCategories = _context.Categories
                .Where(c => c.ParentCategoryId == null) // Sadece ana kategoriler
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList()
        };

        return View(vm);
    }



    [HttpPost]
    public IActionResult Create(CategoryCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.ParentCategories = BuildCategorySelectList(); // burada da doðru liste çaðrýlmalý
            return View(model);
        }


        var category = new Category
        {
            Name = model.Name,
            ParentCategoryId = model.ParentCategoryId
        };

        _context.Categories.Add(category);
        _context.SaveChanges();

        return RedirectToAction("Index");
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

    private List<string> GetHierarchicalCategoryList()
    {
        var all = _context.Categories.ToList();
        var result = new List<string>();

        void BuildList(List<Category> all, int? parentId, string prefix = "")
        {
            var children = all.Where(c => c.ParentCategoryId == parentId).ToList();
            foreach (var child in children)
            {
                result.Add($"{prefix}{child.Name}");
                BuildList(all, child.Id, prefix + "--");
            }
        }

        BuildList(all, null);
        return result;
    }

    private List<SelectListItem> BuildCategorySelectList()
    {
        var allCategories = _context.Categories.ToList();
        var result = new List<SelectListItem>();

        void BuildList(int? parentId, string prefix)
        {
            var children = allCategories.Where(c => c.ParentCategoryId == parentId).OrderBy(c => c.Name).ToList();

            foreach (var category in children)
            {
                result.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = prefix + category.Name
                });

                BuildList(category.Id, prefix + "-- ");
            }
        }

        BuildList(null, "");
        return result;
    }





}
