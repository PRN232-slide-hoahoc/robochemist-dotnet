using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.TopicDTOs.TopicDTOs;

namespace RoboChemist.ExamService.Service.HttpClients
{
    /// <summary>
    /// Interface for Slide Service HTTP client
    /// Provides contract for service-to-service communication with Slide Service
    /// </summary>
    public interface ISlideServiceClient
    {
        /// <summary>
        /// Get topic by ID from Slide Service
        /// </summary>
        /// <param name="topicId">Topic unique identifier</param>
        /// <returns>API response containing topic details</returns>
        Task<ApiResponse<TopicDto>?> GetTopicByIdAsync(Guid topicId);

        // TODO: Add more method signatures as needed
        // Task<ApiResponse<SyllabusDto>?> GetSyllabusByIdAsync(Guid syllabusId);
        // Task<ApiResponse<List<TopicDto>>?> GetTopicsByGradeAsync(Guid gradeId);
    }
}
