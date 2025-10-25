using Microsoft.AspNetCore.Mvc;

namespace RoboChemist.ApiGateway.Controllers
{
    /// <summary>
    /// Gateway Info Controller - Hiển thị thông tin routing
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GatewayController : ControllerBase
    {
        /// <summary>
        /// Lấy thông tin về API Gateway và routes
        /// </summary>
        [HttpGet("info")]
        public IActionResult GetGatewayInfo()
        {
            return Ok(new
            {
                name = "RoboChemist API Gateway",
                version = "1.0.0",
                description = "API Gateway sử dụng Ocelot để route requests đến các microservices",
                routes = new
                {
                    auth = new
                    {
                        path = "/auth/*",
                        service = "AuthService",
                        port = 7188,
                        requiresAuth = false,
                        description = "Authentication endpoints (register, login)"
                    },
                    slides = new
                    {
                        path = "/slides/*",
                        service = "SlidesService",
                        port = 7205,
                        requiresAuth = true,
                        description = "Slides management endpoints"
                    },
                    exam = new
                    {
                        path = "/exam/*",
                        service = "ExamService",
                        port = 7002,
                        requiresAuth = true,
                        description = "Exam management endpoints"
                    },
                    wallet = new
                    {
                        path = "/wallet/*",
                        service = "WalletService",
                        port = 7100,
                        requiresAuth = true,
                        description = "Wallet management endpoints"
                    }
                },
                usage = new
                {
                    step1 = "POST /auth/register - Đăng ký tài khoản",
                    step2 = "POST /auth/login - Đăng nhập và nhận JWT token",
                    step3 = "Sử dụng token với header: Authorization: Bearer {token}",
                    step4 = "Gọi protected APIs: /slides/*, /exam/*, /wallet/*"
                }
            });
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                uptime = Environment.TickCount64 / 1000 + " seconds"
            });
        }

        /// <summary>
        /// Kiểm tra các downstream services
        /// </summary>
        [HttpGet("services/status")]
        public async Task<IActionResult> GetServicesStatus()
        {
            var services = new List<object>();
            var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(2);

            // Check AuthService
            try
            {
                var response = await httpClient.GetAsync("https://localhost:7188/api/user/public");
                services.Add(new
                {
                    name = "AuthService",
                    port = 7188,
                    status = response.IsSuccessStatusCode ? "online" : "offline",
                    statusCode = (int)response.StatusCode
                });
            }
            catch
            {
                services.Add(new { name = "AuthService", port = 7188, status = "offline" });
            }

            // Check SlidesService
            try
            {
                var response = await httpClient.GetAsync("https://localhost:7205/swagger/index.html");
                services.Add(new
                {
                    name = "SlidesService",
                    port = 7205,
                    status = response.IsSuccessStatusCode ? "online" : "offline",
                    statusCode = (int)response.StatusCode
                });
            }
            catch
            {
                services.Add(new { name = "SlidesService", port = 7205, status = "offline" });
            }

            // Check ExamService
            try
            {
                var response = await httpClient.GetAsync("https://localhost:7002/swagger/index.html");
                services.Add(new
                {
                    name = "ExamService",
                    port = 7002,
                    status = response.IsSuccessStatusCode ? "online" : "offline",
                    statusCode = (int)response.StatusCode
                });
            }
            catch
            {
                services.Add(new { name = "ExamService", port = 7002, status = "offline" });
            }

            // Check WalletService
            try
            {
                var response = await httpClient.GetAsync("https://localhost:7100/swagger/index.html");
                services.Add(new
                {
                    name = "WalletService",
                    port = 7100,
                    status = response.IsSuccessStatusCode ? "online" : "offline",
                    statusCode = (int)response.StatusCode
                });
            }
            catch
            {
                services.Add(new { name = "WalletService", port = 7100, status = "offline" });
            }

            return Ok(new
            {
                timestamp = DateTime.UtcNow,
                services = services
            });
        }
    }
}
