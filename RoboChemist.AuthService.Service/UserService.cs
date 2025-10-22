// Services/UserService.cs - Debug version
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

            // DEBUG: In ra để kiểm tra
            Console.WriteLine($"[DEBUG] User found: {user.Email}");
            Console.WriteLine($"[DEBUG] Password hash from DB: {user.PasswordHash}");
            Console.WriteLine($"[DEBUG] Password hash length: {user.PasswordHash?.Length}");
            Console.WriteLine($"[DEBUG] Input password: {request.Password}");

            // Kiểm tra password
            bool isPasswordValid = false;

            try
            {
                // Thử verify với BCrypt
                if (user.PasswordHash.StartsWith("$2a$") || user.PasswordHash.StartsWith("$2b$"))
                {
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
                    Console.WriteLine($"[DEBUG] BCrypt verify result: {isPasswordValid}");
                }
                else
                {
                    // Nếu không phải BCrypt hash, so sánh trực tiếp (KHÔNG AN TOÀN - chỉ để debug)
                    Console.WriteLine("[WARNING] Password is not BCrypt hash! Using plain text comparison (UNSAFE!)");
                    isPasswordValid = user.PasswordHash == request.Password;
                    Console.WriteLine($"[DEBUG] Plain text comparison result: {isPasswordValid}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Password verification failed: {ex.Message}");
                throw new UnauthorizedAccessException("Lỗi xác thực mật khẩu: " + ex.Message);
            }

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

            // Hash password với BCrypt
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            Console.WriteLine($"[DEBUG] New password hash: {hashedPassword}");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Fullname = request.Fullname,
                Email = request.Email,
                PasswordHash = hashedPassword,
                Phone = request.Phone,
                Status = "Active",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
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