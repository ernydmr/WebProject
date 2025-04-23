using System.ComponentModel.DataAnnotations;
using WebProject.Models;


namespace WebProject.ViewModels;
public class RegisterViewModel
{
    [Required(ErrorMessage = "Kullanýcý adý zorunludur.")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Kullanýcý adý 3-20 karakter olmalý.")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Þifre zorunludur.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Þifre en az 6 karakter olmalýdýr.")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Þifre tekrarý zorunludur.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Þifreler uyuþmuyor.")]
    public string ConfirmPassword { get; set; }
}
