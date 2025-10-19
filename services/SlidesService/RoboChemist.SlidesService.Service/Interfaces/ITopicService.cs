using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.GradeDTOs.GradeDTOs;
using static RoboChemist.Shared.DTOs.TopicDTOs.TopicDTOs;

namespace RoboChemist.SlidesService.Service.Interfaces
{
    public interface ITopicService
    {
        /// <summary>
        /// Retrieves a list of topics.
        /// </summary>
        /// <returns>An <see cref="ApiResponse{T}"/> containing a list of <see cref="GetGradeDto"/> objects representing the
        /// topics. The response includes metadata such as success status and error messages, if any.</returns>
        Task<ApiResponse<List<GetTopicDto>>> GetTopicsAsync(Guid? gradeId);

        /// <summary>
        /// Create a new topic.
        /// </summary>
        /// <param name="request">The data for the new topic.</param>
        /// <returns>An <see cref="ApiResponse{T}"/> containing a list of <see cref="GetGradeDto"/> objects representing the
        /// topics. The response includes metadata such as success status and error messages, if any.</returns>
        Task<ApiResponse<GetTopicDto>> CreateTopicAsync(CreateTopicDto request);

        /// <summary>
        /// Retrieves a topic by its unique identifier.
        /// </summary>
        /// <remarks>Use this method to fetch detailed information about a specific topic. Ensure that the
        /// provided gradeId corresponds to an existing topic in the system.</remarks>
        /// <param name="gradeId">The unique identifier of the topic to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an  ApiResponse{T} object with
        /// the topic details as a GetTopicDto  if the topic is found, or an appropriate error response if not.</returns>
        Task<ApiResponse<GetTopicDto>> GetTopicByIdAsync(Guid gradeId);
    }
}
