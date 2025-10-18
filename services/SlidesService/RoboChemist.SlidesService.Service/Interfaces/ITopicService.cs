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
        Task<ApiResponse<List<GetTopicDto>>> GetTopics(Guid? gradeId);   
    }
}
