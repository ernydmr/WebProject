namespace WebProject.Models;

    public class Category
{

    public int Id { get; set; }

    public string Name { get; set; }

    public int? ParentCategoryId { get; set; }

    public List<Category>? SubCategories { get; set; }

    public List<Product>? Products { get; set; }

    public Category? ParentCategory { get; set; }

}