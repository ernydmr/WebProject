using System.ComponentModel.DataAnnotations;
using WebProject.Models;

namespace WebProject.ViewModels;

public class OrderViewModel
{
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Lütfen bir adres seçiniz")]
    public int SelectedAddressId { get; set; }

    [Required(ErrorMessage = "Lütfen adet seçiniz")]
    [Range(1, 100, ErrorMessage = "Adet 1 ile 100 arasında olmalıdır")]
    public int Quantity { get; set; } = 1;

    public List<Address>? Addresses { get; set; }
}
