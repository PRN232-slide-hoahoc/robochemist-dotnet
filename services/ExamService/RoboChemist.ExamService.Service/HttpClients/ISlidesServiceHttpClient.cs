using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.TopicDTOs.TopicDTOs;

namespace RoboChemist.ExamService.Service.HttpClients
{
    /// <summary>
    /// Interface for Slide Service HTTP client
    /// Provides contract for service-to-service communication with Slide Service
    /// </summary>
    public interface ISlidesServiceHttpClient
    {
        /// <summary>
        /// Get topic by ID from Slide Service
        /// </summary>
        /// <param name="topicId">Topic unique identifier</param>
        /// <returns>API response containing topic details or null if not found</returns>
        Task<ApiResponse<TopicDto>?> GetTopicByIdAsync(Guid topicId);
    }
}
