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
                
                if (!string.IsNullOrEmpty(authToken))
                {
                    httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                }
                
                var url = $"/slides/v1/topics/{topicId}";
                var response = await httpClient.GetAsync(url);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
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

                var url = $"/slides/v1/topics/{topicId}";
                var response = await httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = System.Text.Json.JsonSerializer.Deserialize<RoboChemist.Shared.DTOs.Common.ApiResponse<RoboChemist.Shared.DTOs.TopicDTOs.TopicDTOs.TopicDto>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return apiResponse?.Data;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
