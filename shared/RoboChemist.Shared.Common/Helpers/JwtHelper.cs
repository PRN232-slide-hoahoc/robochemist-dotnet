using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace RoboChemist.Shared.Common.Helpers
{
    /// <summary>
    /// Helper class để xử lý JWT token và claims
    /// </summary>
    public static class JwtHelper
    {
        /// <summary>
        /// Lấy UserId từ JWT token (ClaimTypes.NameIdentifier)
        /// </summary>
        /// <param name="user">ClaimsPrincipal từ Controller (User property)</param>
        /// <param name="userId">Output userId nếu parse thành công</param>
        /// <returns>True nếu lấy được userId, False nếu không</returns>
        public static bool TryGetUserId(ClaimsPrincipal user, out Guid userId)
        {
            userId = Guid.Empty;
            
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return false;
            }

            return Guid.TryParse(userIdClaim, out userId);
        }

        /// <summary>
        /// Lấy Email từ JWT token (ClaimTypes.Email)
        /// </summary>
        public static string? GetUserEmail(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value;
        }

        /// <summary>
        /// Lấy Name từ JWT token (ClaimTypes.Name)
        /// </summary>
        public static string? GetUserName(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }

        /// <summary>
        /// Lấy Role từ JWT token (ClaimTypes.Role)
        /// </summary>
        public static string? GetUserRole(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value;
        }

        /// <summary>
        /// Lấy Bearer token từ Authorization header
        /// </summary>
        /// <param name="request">HttpRequest từ Controller</param>
        /// <returns>Token string (không có "Bearer " prefix), hoặc null nếu không có</returns>
        public static string? GetAuthToken(HttpRequest request)
        {
            var authHeader = request.Headers["Authorization"].ToString();
            
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return authHeader.Substring("Bearer ".Length).Trim();
        }

        /// <summary>
        /// Kiểm tra user có role cụ thể không
        /// </summary>
        public static bool HasRole(ClaimsPrincipal user, string role)
        {
            var userRole = GetUserRole(user);
            return !string.IsNullOrEmpty(userRole) && userRole.Equals(role, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Kiểm tra user có một trong các roles không
        /// </summary>
        public static bool HasAnyRole(ClaimsPrincipal user, params string[] roles)
        {
            var userRole = GetUserRole(user);
            if (string.IsNullOrEmpty(userRole))
            {
                return false;
            }

            return roles.Any(r => r.Equals(userRole, StringComparison.OrdinalIgnoreCase));
        }
    }
}
