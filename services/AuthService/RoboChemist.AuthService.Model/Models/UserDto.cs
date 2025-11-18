using System;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;


namespace RoboChemist.AuthService.Model.Models
{
    public class UserDto
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Fullname { get; set; } = null!;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [Phone]
        [StringLength(15)]
        public string? Phone { get; set; }
        [Column("role")] // 👈 THÊM DÒNG NÀY
        public string Role { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
