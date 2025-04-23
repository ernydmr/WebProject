using System.ComponentModel.DataAnnotations;
using WebProject.Models;


namespace WebProject.ViewModels;
public class LoginViewModel
{
    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Ge�erli bir e-posta adresi giriniz.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "�ifre zorunludur.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}
