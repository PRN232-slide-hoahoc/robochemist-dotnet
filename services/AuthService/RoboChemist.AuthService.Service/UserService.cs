// Services/UserService.cs - Plain Text Password (KHÔNG AN TOÀN!)
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using RoboChemist.AuthService.Model.Models;
using RoboChemist.AuthService.Repository;

namespace RoboChemist.AuthService.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;

        // JWT settings - ĐỌC TỪ ENVIRONMENT VARIABLES
        private readonly string SecretKey;
        private readonly string Issuer;
        private readonly string Audience;
        private const int ExpirationMinutes = 60;

        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
            
            // Đọc từ biến môi trường (.env)
            SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET") 
                ?? throw new Exception("JWT_SECRET not found in environment variables!");
            Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
                ?? throw new Exception("JWT_ISSUER not found in environment variables!");
            Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
                ?? throw new Exception("JWT_AUDIENCE not found in environment variables!");
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng");
            }

            // ✅ So sánh mật khẩu bằng BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
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
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Fullname = request.Fullname,
                Email = request.Email,
                PasswordHash = hashedPassword, // Lưu plain text (KHÔNG AN TOÀN!)
                Phone = request.Phone,
                Role = "User",
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
                Role = user.Role,
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
                new Claim(ClaimTypes.Role, user.Role),
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