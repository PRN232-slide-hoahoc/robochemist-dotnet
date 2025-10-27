// Services/UserService.cs - Plain Text Password (KHÔNG AN TOÀN!)
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

using RoboChemist.AuthService.Model.Models;
using RoboChemist.AuthService.Repository;

namespace RoboChemist.AuthService.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;

        // Hardcode JWT settings
        private const string SecretKey = "YourSuperSecretKeyMinimum32CharactersLong!@#$%^&*()";
        private const string Issuer = "RoboChemist.AuthService";
        private const string Audience = "RoboChemist.Client";
        private const int ExpirationMinutes = 60;

        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng");
            }

            // So sánh plain text password
            if (user.PasswordHash != request.Password)
            {
                throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng");
            }

            if (user.IsActive != true)
            {
                throw new UnauthorizedAccessException("Tài khoản đã bị vô hiệu hóa");
            }

            var token = GenerateToken(user);

            return new AuthResponse
            {
                UserId = user.Id,
                Fullname = user.Fullname,
                Email = user.Email,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(ExpirationMinutes)
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _userRepository.EmailExistsAsync(request.Email))
            {
                throw new InvalidOperationException("Email đã được sử dụng");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Fullname = request.Fullname,
                Email = request.Email,
                PasswordHash = request.Password, // Lưu plain text (KHÔNG AN TOÀN!)
                Phone = request.Phone,
                Status = "Active",
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _userRepository.CreateAsync(user);

            var token = GenerateToken(user);

            return new AuthResponse
            {
                UserId = user.Id,
                Fullname = user.Fullname,
                Email = user.Email,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(ExpirationMinutes)
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return null;

            return new UserDto
            {
                Id = user.Id,
                Fullname = user.Fullname,
                Email = user.Email,
                Phone = user.Phone,
                Status = user.Status,
                IsActive = user.IsActive
            };
        }

        // JWT Logic
        private string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Fullname),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(ExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}