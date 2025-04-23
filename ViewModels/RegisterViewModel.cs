using System.ComponentModel.DataAnnotations;
using WebProject.Models;


namespace WebProject.ViewModels;
public class RegisterViewModel
{
    [Required(ErrorMessage = "Kullan�c� ad� zorunludur.")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Kullan�c� ad� 3-20 karakter olmal�.")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Ge�erli bir e-posta adresi giriniz.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "�ifre zorunludur.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "�ifre en az 6 karakter olmal�d�r.")]
    public string Password { get; set; }

    [Required(ErrorMessage = "�ifre tekrar� zorunludur.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "�ifreler uyu�muyor.")]
    public string ConfirmPassword { get; set; }
}
