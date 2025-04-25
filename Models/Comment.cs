using System.ComponentModel.DataAnnotations;

namespace WebProject.Models;

public class Comment
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Kullan�c� ad� zorunludur.")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Yorum i�eri�i bo� olamaz.")]
    public string Content { get; set; }

    [Range(1, 5, ErrorMessage = "Puan 1 ile 5 aras�nda olmal�d�r.")]
    public int Rating { get; set; } // 1-5 aras�
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
