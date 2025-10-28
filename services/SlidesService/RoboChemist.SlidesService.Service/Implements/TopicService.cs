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

        public async Task<ApiResponse<TopicDto>> CreateTopicAsync(CreateTopicDto request)
        {
            try
            {
                var grade = await _uow.Grades.GetByIdAsync(request.GradeId);

                if (grade == null)
                {
                    return ApiResponse<TopicDto>.ErrorResult("Khối lớp không tồn tại");
                }

                var topic = new Topic
                {
                    GradeId = request.GradeId,
                    SortOrder = request.SortOrder,
                    TopicName = request.Name,
                    Description = request.Description
                };
                await _uow.Topics.CreateAsync(topic);

                var topicDto = new TopicDto
                {
                    Id = topic.Id,
                    GradeId = grade.Id,
                    SortOrder = topic.SortOrder ?? 0,
                    GradeName = grade.GradeName,
                    Name = topic.TopicName,
                    Description = topic.Description ?? string.Empty
                };

                return ApiResponse<TopicDto>.SuccessResult(topicDto);
            }
            catch (Exception)
            {
                return ApiResponse<TopicDto>.ErrorResult("Lỗi hệ thống");
            }
        }

        public async Task<ApiResponse<List<TopicDto>>> GetTopicsAsync(Guid? gradeId)
        {
            try
            {
                List<TopicDto> topics = await _uow.Topics.GetFullTopicsAsync(gradeId);

                return ApiResponse<List<TopicDto>>.SuccessResult(topics);
            }
            catch (Exception)
            {
                return ApiResponse<List<TopicDto>>.ErrorResult("Lỗi hệ thống");
            }
        }

        public async Task<ApiResponse<TopicDto>> GetTopicByIdAsync(Guid topicId)
        {
            try
            {
                Topic? topic = await _uow.Topics.GetByIdAsync(topicId);

                if (topic == null)
                {
                    return ApiResponse<TopicDto>.ErrorResult("Không tìm thấy chủ đề với ID đã chọn");
                }

                TopicDto topicDto = new()
                {
                    Id = topic.Id,
                    GradeId = topic.GradeId,
                    GradeName = topic.Grade?.GradeName ?? "N/A",
                    SortOrder = topic.SortOrder ?? 0,
                    Name = topic.TopicName,
                    Description = topic.Description ?? string.Empty
                };

                return ApiResponse<TopicDto>.SuccessResult(topicDto);
            }
            catch (Exception)
            {
                return ApiResponse<TopicDto>.ErrorResult("Lỗi hệ thống");
            }
        }
    }
}
