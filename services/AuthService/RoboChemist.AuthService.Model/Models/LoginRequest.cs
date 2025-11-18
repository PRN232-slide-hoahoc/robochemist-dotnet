using System.ComponentModel.DataAnnotations;

namespace RoboChemist.AuthService.Model.Models
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [MaxLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(1, ErrorMessage = "Mật khẩu phải có ít nhất 1 ký tự")]
        [MaxLength(50, ErrorMessage = "Mật khẩu không được vượt quá 50 ký tự")]
        public string Password { get; set; } = null!;
    }
}
