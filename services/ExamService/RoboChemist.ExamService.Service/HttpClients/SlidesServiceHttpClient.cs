using static RoboChemist.Shared.DTOs.TopicDTOs.TopicDTOs;

namespace RoboChemist.ExamService.Service.HttpClients
{
    /// <summary>
    /// HttpClient để gọi các API của SlidesService qua ApiGateway
    /// </summary>
    public class SlidesServiceHttpClient : ISlidesServiceHttpClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SlidesServiceHttpClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Gọi API GetTopicById của SlidesService để kiểm tra Topic có tồn tại không
        /// </summary>
        /// <param name="topicId">ID của Topic</param>
        /// <param name="authToken">JWT Bearer token</param>
        /// <returns>True nếu Topic tồn tại (status 200), False nếu không tồn tại (404) hoặc lỗi</returns>
        public async Task<bool> GetTopicByIdAsync(Guid topicId, string? authToken = null)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiGateway");
                
                // Thêm Authorization header nếu có token
                if (!string.IsNullOrEmpty(authToken))
                {
                    httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                }
                
                // Gọi API GetTopicById qua ApiGateway
                // Gateway route: /slides/v1/Topic/{id} -> SlidesService
                var url = $"/slides/v1/Topic/{topicId}";
                Console.WriteLine($"[HTTP] Calling SlidesService via Gateway: {httpClient.BaseAddress}{url}");
                
                var response = await httpClient.GetAsync(url);
                
                Console.WriteLine($"[HTTP] Response: {response.StatusCode}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[HTTP] Error content: {errorContent}");
                }
                
                // Trả về true nếu status code là 2xx (thành công)
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HTTP ERROR] Failed to call SlidesService: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gọi SlidesService lấy chi tiết Topic qua ApiGateway và deserialize về Topic DTO
        /// </summary>
        public async Task<TopicDto?> GetTopicAsync(Guid topicId, string? authToken = null)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiGateway");

                if (!string.IsNullOrEmpty(authToken))
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                }

                var url = $"/slides/v1/Topic/{topicId}";
                Console.WriteLine($"[HTTP] Calling SlidesService via Gateway for Topic DTO: {httpClient.BaseAddress}{url}");

                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[HTTP] GetTopicAsync returned status: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = System.Text.Json.JsonSerializer.Deserialize<RoboChemist.Shared.DTOs.Common.ApiResponse<RoboChemist.Shared.DTOs.TopicDTOs.TopicDTOs.TopicDto>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HTTP ERROR] GetTopicAsync failed: {ex.Message}");
                return null;
            }
        }
    }
}
