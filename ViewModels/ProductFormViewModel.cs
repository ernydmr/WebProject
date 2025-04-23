using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using WebProject.Models;

namespace WebProject.ViewModels;

public class ProductFormViewModel
{
    public int? Id { get; set; } // Edit için kullanýlabilir

    [Required]
    public string Name { get; set; }

    [Required]
    [Range(0, 999999)]
    public decimal Price { get; set; }

    public string? Description { get; set; }

    [Range(0, 100000)]
    public int Stock { get; set; }

    public int CategoryId { get; set; }

    public IFormFile? ImageFile { get; set; } // görsel için

    public List<Category> TopCategories { get; set; } = new();
}
