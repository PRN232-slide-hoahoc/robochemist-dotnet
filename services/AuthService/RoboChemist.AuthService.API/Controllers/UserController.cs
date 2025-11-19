using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoboChemist.AuthService.Model.Models;
using RoboChemist.AuthService.Service.Interfaces;
using RoboChemist.Shared.DTOs.Common;
using System.Security.Claims;

namespace RoboChemist.AuthService.API.Controllers
{
    /// <summary>
    /// Controller quản lý người dùng: đăng ký, đăng nhập, lấy thông tin người dùng, validate token.
    /// </summary>
    [ApiController]
    [Route("api/v1/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Khởi tạo UserController.
        /// </summary>
        /// <param name="userService">Service xử lý nghiệp vụ người dùng.</param>
        public UserController(IUserService userService)
        {
            _userService = userService;
        }


        /// <summary>
        /// Đăng ký tài khoản mới.
        /// </summary>
        /// <param name="request">Thông tin đăng ký gồm email, password, name...</param>
        /// <returns>ApiResponse chứa AuthResponse (JWT + User info)</returns>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var response = await _userService.RegisterAsync(request);
                return Ok(ApiResponse<AuthResponse>.SuccessResult(response, "Đăng ký thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AuthResponse>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    ApiResponse<AuthResponse>.ErrorResult("Có lỗi xảy ra trong quá trình đăng ký", [ex.Message]));
            }
        }


        /// <summary>
        /// Đăng nhập và trả về JWT token.
        /// </summary>
        /// <param name="request">Thông tin đăng nhập (email + password).</param>
        /// <returns>ApiResponse chứa AuthResponse (JWT + User info)</returns>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _userService.LoginAsync(request);
                return Ok(ApiResponse<AuthResponse>.SuccessResult(response, "Đăng nhập thành công"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<AuthResponse>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    ApiResponse<AuthResponse>.ErrorResult("Có lỗi xảy ra trong quá trình đăng nhập", [ex.Message]));
            }
        }


        /// <summary>
        /// Lấy thông tin người dùng hiện tại dựa vào token.
        /// </summary>
        /// <returns>ApiResponse chứa thông tin UserDto.</returns>
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(ApiResponse<UserDto>.ErrorResult("Token không hợp lệ"));
                }

                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(ApiResponse<UserDto>.ErrorResult("Không tìm thấy người dùng"));
                }

                return ApiResponse<UserDto>.SuccessResult(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    ApiResponse<UserDto>.ErrorResult("Có lỗi xảy ra", [ex.Message]));
            }
        }


        /// <summary>
        /// Xác thực token xem có hợp lệ hay không.
        /// </summary>
        /// <returns>Thông tin user từ token.</returns>
        [Authorize]
        [HttpGet("validate-token")]
        public IActionResult ValidateToken()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var name = User.FindFirst(ClaimTypes.Name)?.Value;

            return Ok(new
            {
                success = true,
                message = "Token hợp lệ",
                data = new
                {
                    userId,
                    email,
                    name
                }
            });
        }


        /// <summary>
        /// Lấy thông tin người dùng theo ID.
        /// </summary>
        /// <param name="id">ID của người dùng.</param>
        /// <returns>Thông tin người dùng nếu tồn tại.</returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);

                if (user == null)
                {
                    return NotFound(ApiResponse<UserDto>.ErrorResult("Không tìm thấy người dùng"));
                }

                return Ok(ApiResponse<UserDto>.SuccessResult(user));
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    ApiResponse<UserDto>.ErrorResult("Có lỗi xảy ra", [ex.Message]));
            }
        }


        /// <summary>
        /// Endpoint công khai dành cho test, không yêu cầu token.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("public")]
        public IActionResult PublicEndpoint()
        {
            return Ok(ApiResponse<string>.SuccessResult("Đây là endpoint công khai, không cần token"));
        }


        /// <summary>
        /// Endpoint bảo vệ, chỉ truy cập khi có JWT.
        /// </summary>
        [Authorize]
        [HttpGet("protected")]
        public IActionResult ProtectedEndpoint()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            return Ok(ApiResponse<object>.SuccessResult(new
            {
                userId,
                email
            }, "Đây là endpoint được bảo vệ"));
        }
    }
}
