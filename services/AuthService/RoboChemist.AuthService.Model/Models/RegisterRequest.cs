using System.ComponentModel.DataAnnotations;

namespace RoboChemist.AuthService.Model.Models
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Họ và tên không được để trống")]
        [MaxLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
        public string Fullname { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [MaxLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [MaxLength(50, ErrorMessage = "Mật khẩu không được vượt quá 50 ký tự")]
        public string Password { get; set; } = null!;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự")]
        public string? Phone { get; set; }
    }
}
