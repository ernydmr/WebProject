using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebProject.Data;
using WebProject.Models;
using WebProject.ViewModels;
using System.Security.Claims;



namespace WebProject.Controllers
{
    [Authorize(Roles = "admin")]
    public class  AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public AdminController(AppDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();

        }

        [HttpGet]
        public IActionResult ProductList(string search)
        {
            var productsQuery = _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(search) ||
                    p.Description.Contains(search) ||
                    p.User.UserName.Contains(search));
            }

            ViewBag.Search = search;

            var products = productsQuery.ToList();
            return View(products);
        }


        [HttpGet]
        public IActionResult FilteredProducts(string term)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .Where(p => p.Name.Contains(term) ||
                            p.Description.Contains(term) ||
                            p.User.UserName.Contains(term))
                .Select(p => new {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Stock,
                    p.Description,
                    Category = p.Category.Name,
                    Seller = p.User.UserName,
                    p.ImageUrl
                })
                .ToList();

            return Json(products);
        }



        [HttpGet]
        public async Task<IActionResult> Users(string search)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                usersQuery = usersQuery.Where(u =>
                    u.Email.Contains(search) || u.Id.Contains(search));
            }

            var users = await usersQuery.ToListAsync();

            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles;
            }

            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            ViewBag.UserRoles = userRoles;
            ViewBag.AllRoles = allRoles;
            ViewBag.Search = search;

            return View(users);
        }


        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, string selectedRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && !(await _userManager.IsInRoleAsync(user, selectedRole)))
            {
                await _userManager.AddToRoleAsync(user, selectedRole);
            }
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _userManager.IsInRoleAsync(user, role))
            {
                await _userManager.RemoveFromRoleAsync(user, role);
            }
            return RedirectToAction("Users");
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
            return RedirectToAction("ProductList");
        }

        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order ==null)
            {
                return NotFound();
            }
            order.Status = newStatus;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Orders));
        }

    }


}