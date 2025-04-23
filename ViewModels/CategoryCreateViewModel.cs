using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebProject.Models;

namespace WebProject.ViewModels
{
    public class CategoryCreateViewModel
    {
        [Required(ErrorMessage = "Kategori adý zorunludur.")]
        public string Name { get; set; }

        [Display(Name = "Üst Kategori")]
        public int? ParentCategoryId { get; set; }

        public List<SelectListItem> ParentCategories { get; set; } = new();
    }
}
