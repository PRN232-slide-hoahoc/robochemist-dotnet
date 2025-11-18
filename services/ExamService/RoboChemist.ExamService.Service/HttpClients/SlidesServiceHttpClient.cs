using Microsoft.AspNetCore.Http;
using RoboChemist.Shared.DTOs.Common;
using System.Text.Json;
using static RoboChemist.Shared.DTOs.TopicDTOs.TopicDTOs;

namespace RoboChemist.ExamService.Service.HttpClients
{
    /// <summary>
    /// Typed HTTP client for communicating with Slide Service
    /// Handles authentication token forwarding for service-to-service calls
    /// </summary>
    public class SlidesServiceHttpClient : ISlidesServiceHttpClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SlidesServiceHttpClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Forward Authorization Bearer token from current request to HttpClient
        /// </summary>
        private void AuthorizeHttpClient(HttpClient httpClient)
        {
            var authToken = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"]
                    .ToString()
                    .Replace("Bearer ", "");

            if (!string.IsNullOrEmpty(authToken))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            }
        }

        /// <summary>
        /// Get topic by ID from Slide Service via API Gateway
        /// </summary>
        /// <param name="topicId">Topic unique identifier</param>
        /// <returns>API response containing topic details or null if request fails</returns>
        public async Task<ApiResponse<TopicDto>?> GetTopicByIdAsync(Guid topicId)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiGateway");
                AuthorizeHttpClient(httpClient);

                var url = $"/slides/v1/topics/{topicId}";
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<TopicDto>>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                return apiResponse;
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Failed to get topic {topicId} from Slide Service", ex);
            }
        }

        /// <summary>
        /// Get multiple topics by IDs from Slide Service (BATCH GET - Tránh N+1 query)
        /// </summary>
        /// <param name="topicIds">List of topic IDs</param>
        /// <returns>Dictionary mapping TopicId to TopicDto</returns>
        public async Task<Dictionary<Guid, TopicDto>> GetTopicsByIdsAsync(IEnumerable<Guid> topicIds)
        {
            var result = new Dictionary<Guid, TopicDto>();
            
            if (topicIds == null || !topicIds.Any())
            {
                return result;
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiGateway");
                AuthorizeHttpClient(httpClient);

                // Gọi từng topic (tạm thời - nếu Slides Service có batch endpoint thì dùng batch)
                // TODO: Nếu SlidesService có endpoint GET /topics?ids=guid1,guid2,guid3 thì dùng endpoint đó
                var tasks = topicIds.Distinct().Select(async topicId =>
                {
                    try
                    {
                        var response = await GetTopicByIdAsync(topicId);
                        if (response?.Success == true && response.Data != null)
                        {
                            return new KeyValuePair<Guid, TopicDto>(topicId, response.Data);
                        }
                    }
                    catch { }
                    return new KeyValuePair<Guid, TopicDto>(topicId, null!);
                });

                var responses = await Task.WhenAll(tasks);
                
                foreach (var kvp in responses.Where(x => x.Value != null))
                {
                    result[kvp.Key] = kvp.Value;
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Failed to batch get topics from Slide Service", ex);
            }
        }
    }
}
