using System.ComponentModel.DataAnnotations;

namespace WebProject.Models;

public class Comment
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Kullanýcý adý zorunludur.")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Yorum içeriði boþ olamaz.")]
    public string Content { get; set; }

    [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasýnda olmalýdýr.")]
    public int Rating { get; set; } // 1-5 arasý
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
