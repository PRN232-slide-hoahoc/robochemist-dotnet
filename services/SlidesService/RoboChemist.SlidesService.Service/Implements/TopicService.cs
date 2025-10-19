using RoboChemist.Shared.DTOs.Common;
using RoboChemist.SlidesService.Model.Models;
using RoboChemist.SlidesService.Repository.Interfaces;
using RoboChemist.SlidesService.Service.Interfaces;
using static RoboChemist.Shared.DTOs.TopicDTOs.TopicDTOs;

namespace RoboChemist.SlidesService.Service.Implements
{
    public class TopicService : ITopicService
    {
        private readonly IUnitOfWork _uow;

        public TopicService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ApiResponse<GetTopicDto>> CreateTopicAsync(CreateTopicDto request)
        {
            try
            {
                var grade = await _uow.Grades.GetByIdAsync(request.GradeId);

                if (grade == null)
                {
                    return ApiResponse<GetTopicDto>.ErrorResult("Khối lớp không tồn tại");
                }

                var topic = new Topic
                {
                    GradeId = request.GradeId,
                    SortOrder = request.SortOrder,
                    TopicName = request.Name,
                    Description = request.Description
                };
                await _uow.Topics.CreateAsync(topic);

                var topicDto = new GetTopicDto
                {
                    Id = topic.Id,
                    GradeId = grade.Id,
                    SortOrder = topic.SortOrder ?? 0,
                    GradeName = grade.GradeName,
                    Name = topic.TopicName,
                    Description = topic.Description ?? string.Empty
                };

                return ApiResponse<GetTopicDto>.SuccessResult(topicDto);
            }
            catch (Exception)
            {
                return ApiResponse<GetTopicDto>.ErrorResult("Lỗi hệ thống");
            }
        }

        public async Task<ApiResponse<List<GetTopicDto>>> GetTopicsAsync(Guid? gradeId)
        {
            try
            {
                List<GetTopicDto> topics = await _uow.Topics.GetFullTopicsAsync(gradeId);

                return ApiResponse<List<GetTopicDto>>.SuccessResult(topics);
            }
            catch (Exception)
            {
                return ApiResponse<List<GetTopicDto>>.ErrorResult("Lỗi hệ thống");
            }
        }

        public async Task<ApiResponse<GetTopicDto>> GetTopicByIdAsync(Guid topicId)
        {
            try
            {
                Topic? topic = await _uow.Topics.GetByIdAsync(topicId);

                if (topic == null)
                {
                    return ApiResponse<GetTopicDto>.ErrorResult("Không tìm thấy chủ đề với ID đã chọn");
                }

                GetTopicDto topicDto = new()
                {
                    Id = topic.Id,
                    GradeId = topic.GradeId,
                    GradeName = topic.Grade.GradeName,
                    SortOrder = topic.SortOrder ?? 0,
                    Name = topic.TopicName,
                    Description = topic.Description ?? string.Empty
                };

                return ApiResponse<GetTopicDto>.SuccessResult(topicDto);
            }
            catch (Exception)
            {
                return ApiResponse<GetTopicDto>.ErrorResult("Lỗi hệ thống");
            }
        }
    }
}
