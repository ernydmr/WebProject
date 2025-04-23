using System.ComponentModel.DataAnnotations;
using WebProject.Models;


namespace WebProject.ViewModels;
public class ProductDetailViewModel
{
    public Product Product { get; set; }
    public List<Comment> Comments { get; set; }
    public Comment NewComment { get; set; }
}
