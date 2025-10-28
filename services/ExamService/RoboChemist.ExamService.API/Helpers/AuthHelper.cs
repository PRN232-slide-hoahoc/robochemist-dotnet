using System.Security.Claims;

namespace RoboChemist.ExamService.API.Helpers
{
    /// <summary>
    /// Helper class để lấy thông tin user từ JWT token
    /// </summary>
    public static class AuthHelper
    {
        /// <summary>
        /// Lấy UserId từ JWT claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal từ HttpContext.User</param>
        /// <returns>UserId dạng Guid, hoặc Guid.Empty nếu không tìm thấy</returns>
        public static Guid GetUserId(ClaimsPrincipal user)
        {
            // Thử lấy từ claim "sub" (subject - standard JWT claim)
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) 
                           ?? user.FindFirst("sub")
                           ?? user.FindFirst("userId");

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Lấy Username từ JWT claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal từ HttpContext.User</param>
        /// <returns>Username hoặc string.Empty nếu không tìm thấy</returns>
        public static string GetUsername(ClaimsPrincipal user)
        {
            var usernameClaim = user.FindFirst(ClaimTypes.Name) 
                             ?? user.FindFirst("username")
                             ?? user.FindFirst("name");

            return usernameClaim?.Value ?? string.Empty;
        }

        /// <summary>
        /// Lấy Email từ JWT claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal từ HttpContext.User</param>
        /// <returns>Email hoặc string.Empty nếu không tìm thấy</returns>
        public static string GetEmail(ClaimsPrincipal user)
        {
            var emailClaim = user.FindFirst(ClaimTypes.Email) 
                          ?? user.FindFirst("email");

            return emailClaim?.Value ?? string.Empty;
        }

        /// <summary>
        /// Lấy Role từ JWT claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal từ HttpContext.User</param>
        /// <returns>Role hoặc string.Empty nếu không tìm thấy</returns>
        public static string GetRole(ClaimsPrincipal user)
        {
            var roleClaim = user.FindFirst(ClaimTypes.Role) 
                         ?? user.FindFirst("role");

            return roleClaim?.Value ?? string.Empty;
        }

        /// <summary>
        /// Kiểm tra user có role cụ thể hay không
        /// </summary>
        /// <param name="user">ClaimsPrincipal từ HttpContext.User</param>
        /// <param name="role">Role cần kiểm tra</param>
        /// <returns>True nếu user có role đó</returns>
        public static bool HasRole(ClaimsPrincipal user, string role)
        {
            return user.IsInRole(role);
        }
    }
}
