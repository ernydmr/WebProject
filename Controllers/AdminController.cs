using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebProject.Data;
using WebProject.Models;
using WebProject.ViewModels;


namespace WebProject.Controllers
{
    [Authorize(Roles = "admin")]
    public class  AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var products = _context.Products
                .Include(p => p.Category)
                .ToList();
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
                ImageUrl = imagePath
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            var categories = _context.Categories.ToList();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(Product updatedProduct, IFormFile? imageFile)
        {
            var product = _context.Products.Find(updatedProduct.Id);
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

                return RedirectToAction("Index");
            }

            var categories = _context.Categories.ToList();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", updatedProduct.CategoryId);

            return View(updatedProduct);
        }




        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

    }


}