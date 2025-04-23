
namespace WebProject.Models;
public class Comment
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string UserName { get; set; }
    public string Content { get; set; }
    public int Rating { get; set; } // 1-5 arasý
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
