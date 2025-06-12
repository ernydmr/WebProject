using System.ComponentModel.DataAnnotations;

namespace WebProject.Models;

public class Address
{
    public int Id { get; set; }

    public string UserId { get; set; }

    public ApplicationUser User { get; set; }

    [Required]
    public string FullAddress { get; set; }

    [Required]
    public string City { get; set; }

    [Required]
    public string PostalCode { get; set; }

    [Phone]
    public string Phone { get; set; }

}