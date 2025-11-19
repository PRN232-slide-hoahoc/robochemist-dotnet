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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _authServiceUrl;
        private readonly string _slidesServiceUrl;
        private readonly string _examServiceUrl;
        private readonly string _walletServiceUrl;
        private readonly string _templateServiceUrl;

        public GatewayController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _authServiceUrl = Environment.GetEnvironmentVariable("AUTH_SERVICE_URL") ?? "https://localhost:7188";
            _slidesServiceUrl = Environment.GetEnvironmentVariable("SLIDE_SERVICE_URL") ?? "https://localhost:7205";
            _examServiceUrl = Environment.GetEnvironmentVariable("EXAM_SERVICE_URL") ?? "https://localhost:7002";
            _walletServiceUrl = Environment.GetEnvironmentVariable("WALLET_SERVICE_URL") ?? "https://localhost:7100";
            _templateServiceUrl = Environment.GetEnvironmentVariable("TEMPLATE_SERVICE_URL") ?? "https://localhost:7206";
        }

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
            var httpClient = _httpClientFactory.CreateClient("ServiceHealthCheck");

            // Check all services in parallel
            var serviceChecks = new[]
            {
                CheckServiceAsync(httpClient, "AuthService", _authServiceUrl, "swagger/v1/swagger.json"),
                CheckServiceAsync(httpClient, "SlidesService", _slidesServiceUrl, "swagger/v1/swagger.json"),
                CheckServiceAsync(httpClient, "ExamService", _examServiceUrl, "swagger/v1/swagger.json"),
                CheckServiceAsync(httpClient, "WalletService", _walletServiceUrl, "swagger/v1/swagger.json"),
                CheckServiceAsync(httpClient, "TemplateService", _templateServiceUrl, "swagger/v1/swagger.json")
            };

            var services = await Task.WhenAll(serviceChecks);

            return Ok(new
            {
                timestamp = DateTime.UtcNow,
                services = services
            });
        }

        private async Task<object> CheckServiceAsync(HttpClient httpClient, string serviceName, string baseUrl, string endpoint)
        {
            try
            {
                var url = $"{baseUrl}/{endpoint}";
                var response = await httpClient.GetAsync(url);
                var port = new Uri(baseUrl).Port;
                
                return new
                {
                    name = serviceName,
                    port = port,
                    status = response.IsSuccessStatusCode ? "online" : "offline",
                    statusCode = (int)response.StatusCode,
                    url = baseUrl
                };
            }
            catch (Exception ex)
            {
                var port = new Uri(baseUrl).Port;
                return new
                {
                    name = serviceName,
                    port = port,
                    status = "offline",
                    error = ex.Message,
                    url = baseUrl
                };
            }
        }
    }
}
