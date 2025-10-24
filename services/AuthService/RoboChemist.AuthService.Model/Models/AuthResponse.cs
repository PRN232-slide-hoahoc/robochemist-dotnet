using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboChemist.AuthService.Model.Models
{
    public class AuthResponse
    {
        public Guid UserId { get; set; }
        public string Fullname { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}
