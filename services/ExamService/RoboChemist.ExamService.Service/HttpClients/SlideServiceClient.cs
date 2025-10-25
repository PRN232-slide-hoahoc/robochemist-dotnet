using RoboChemist.Shared.DTOs.Common;
using System.Net.Http.Json;
using static RoboChemist.Shared.DTOs.TopicDTOs.TopicDTOs;

namespace RoboChemist.ExamService.Service.HttpClients
{
    /// <summary>
    /// Typed HTTP client for communicating with Slide Service
    /// Follows microservices best practices for service-to-service communication
    /// </summary>
    public class SlideServiceClient : ISlideServiceClient
    {
        private readonly HttpClient _httpClient;

        public SlideServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Get topic by ID from Slide Service via API Gateway
        /// </summary>
        /// <param name="topicId">Topic unique identifier</param>
        /// <returns>API response containing topic details</returns>
        public async Task<ApiResponse<TopicDto>?> GetTopicByIdAsync(Guid topicId)
        {
            try
            {
                // ✅ Gọi qua Gateway: /slides/v1/Topic/{topicId}
                // Gateway sẽ route đến SlidesService: /api/v1/Topic/{topicId}
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<TopicDto>>($"/slides/v1/Topic/{topicId}");
                return response;
            }
            catch (HttpRequestException ex)
            {
                // Log the error here if you have ILogger injected
                throw new HttpRequestException($"Failed to get topic {topicId} from Slide Service via Gateway", ex);
            }
        }

        // TODO: Add more methods as needed for other Slide Service endpoints
        // Example: GetGradeByIdAsync, GetSyllabusByIdAsync, GetAllTopicsAsync, etc.
    }
}
