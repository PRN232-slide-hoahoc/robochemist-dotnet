using RoboChemist.Shared.DTOs.Common;
using RoboChemist.SlidesService.Repository.Interfaces;
using RoboChemist.SlidesService.Service.Interfaces;
using static RoboChemist.Shared.DTOs.TopicDTOs.TopicDTOs;

namespace RoboChemist.SlidesService.Service.Implements
{
    public class TopicService : ITopicService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TopicService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ApiResponse<List<GetTopicDto>>> GetTopics(Guid? gradeId)
        {
            try
            {
                List<GetTopicDto> topics = await _unitOfWork.Topics.GetFullTopicsAsync(gradeId);

                return ApiResponse<List<GetTopicDto>>.SuccessResul(topics);
            }
            catch (Exception)
            {
                return ApiResponse<List<GetTopicDto>>.ErrorResult("Lỗi hệ thống");
            }
        }
    }
}
