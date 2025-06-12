using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace WebProject.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(50)]
        public string? FullName { get; set; }

        [DataType(DataType.Date)] //  Doðru yazým
        public DateTime? BirthDate { get; set; }

        public string? Bio { get; set; }

        public string? AvatarPath { get; set; }

        public ICollection<Product>? Products { get; set; }

        public DateTime? LastSeen { get; set; }

        public decimal Balance { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
