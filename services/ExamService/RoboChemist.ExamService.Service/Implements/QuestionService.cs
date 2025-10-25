using RoboChemist.ExamService.Repository.Interfaces;
using RoboChemist.ExamService.Service.Interfaces;
using RoboChemist.ExamService.Service.HttpClients;
using RoboChemist.ExamService.Model.Models;
using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.QuestionDTOs.QuestionDTOs;
using static RoboChemist.Shared.DTOs.TopicDTOs.TopicDTOs;
using Microsoft.EntityFrameworkCore;

namespace RoboChemist.ExamService.Service.Implements
{
    /// <summary>
    /// Service implementation for Question CRUD operations
    /// </summary>
    public class QuestionService : IQuestionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISlideServiceClient _slideServiceClient;

        public QuestionService(IUnitOfWork unitOfWork, ISlideServiceClient slideServiceClient)
        {
            _unitOfWork = unitOfWork;
            _slideServiceClient = slideServiceClient;
        }

        /// <summary>
        /// Create a new question with options
        /// </summary>
        public async Task<ApiResponse<QuestionDto>> CreateQuestionAsync(CreateQuestionDto createQuestionDto)
        {
            try
            {
                // Validate at least one correct answer
                if (!createQuestionDto.Options.Any(o => o.IsCorrect))
                {
                    return ApiResponse<QuestionDto>.ErrorResult("Phải có ít nhất một đáp án đúng");
                }

                // Validate Topic existence by calling Slide Service via typed client
                var topicResponse = await _slideServiceClient.GetTopicByIdAsync(createQuestionDto.TopicId);

                if (topicResponse is null)
                {
                    return ApiResponse<QuestionDto>.ErrorResult("Không nhận được phản hồi từ Slide Service");
                }

                if (!topicResponse.Success || topicResponse.Data is null)
                {
                    var msg = string.IsNullOrWhiteSpace(topicResponse.Message)
                        ? $"Topic không tồn tại: {createQuestionDto.TopicId}"
                        : topicResponse.Message;
                    return ApiResponse<QuestionDto>.ErrorResult(msg);
                }

                // Create Question entity
                var question = new Question
                {
                    QuestionId = Guid.NewGuid(),
                    TopicId = (int)createQuestionDto.TopicId.GetHashCode(), // TODO: Fix TopicId type mismatch
                    QuestionType = createQuestionDto.QuestionType,
                    Question1 = createQuestionDto.QuestionText,
                    Explanation = createQuestionDto.Explanation,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = null // TODO: Get from authenticated user
                };

                // Create Options
                var options = createQuestionDto.Options.Select(opt => new Option
                {
                    OptionId = Guid.NewGuid(),
                    QuestionId = question.QuestionId,
                    Answer = opt.Answer,
                    IsCorrect = opt.IsCorrect,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = null // TODO: Get from authenticated user
                }).ToList();

                question.Options = options;

                // Save to database
                await _unitOfWork.Questions.CreateAsync(question);
                await _unitOfWork.SaveChangesAsync();

                // Return DTO
                var result = new QuestionDto
                {
                    Id = question.QuestionId,
                    TopicId = createQuestionDto.TopicId,
                    TopicName = topicResponse.Data.Name,
                    QuestionType = question.QuestionType,
                    QuestionText = question.Question1,
                    Explanation = question.Explanation,
                    IsActive = question.IsActive ?? true,
                    CreatedAt = question.CreatedAt,
                    Options = options.Select(o => new OptionDto
                    {
                        Id = o.OptionId,
                        Answer = o.Answer,
                        IsCorrect = o.IsCorrect ?? false
                    }).ToList()
                };

                return ApiResponse<QuestionDto>.SuccessResult(result, "Tạo câu hỏi thành công");
            }
            catch (HttpRequestException ex)
            {
                return ApiResponse<QuestionDto>.ErrorResult($"Lỗi kết nối tới Slide Service: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse<QuestionDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a question by ID
        /// </summary>
        public async Task<ApiResponse<QuestionDto>> GetQuestionByIdAsync(Guid id)
        {
            try
            {
                var question = await _unitOfWork.Questions.GetByIdAsync(id);

                if (question == null)
                {
                    return ApiResponse<QuestionDto>.ErrorResult($"Không tìm thấy câu hỏi với ID: {id}");
                }

                // Get topic name from Slide Service
                var topicResponse = await _slideServiceClient.GetTopicByIdAsync(Guid.NewGuid()); // TODO: Fix TopicId conversion
                var topicName = topicResponse?.Success == true ? topicResponse.Data?.Name ?? "" : "";

                var result = new QuestionDto
                {
                    Id = question.QuestionId,
                    TopicId = Guid.NewGuid(), // TODO: Fix TopicId conversion
                    TopicName = topicName,
                    QuestionType = question.QuestionType,
                    QuestionText = question.Question1,
                    Explanation = question.Explanation,
                    IsActive = question.IsActive ?? true,
                    CreatedAt = question.CreatedAt,
                    UpdatedAt = question.UpdatedAt,
                    Options = question.Options.Select(o => new OptionDto
                    {
                        Id = o.OptionId,
                        Answer = o.Answer,
                        IsCorrect = o.IsCorrect ?? false
                    }).ToList()
                };

                return ApiResponse<QuestionDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<QuestionDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all questions, optionally filtered by topic
        /// </summary>
        public async Task<ApiResponse<List<QuestionDto>>> GetQuestionsAsync(Guid? topicId = null)
        {
            try
            {
                var questions = await _unitOfWork.Questions.GetAllAsync();

                if (topicId.HasValue)
                {
                    // TODO: Fix TopicId filter when TopicId type is corrected
                    // questions = questions.Where(q => q.TopicId == topicId.Value).ToList();
                }

                var result = questions.Select(q => new QuestionDto
                {
                    Id = q.QuestionId,
                    TopicId = Guid.NewGuid(), // TODO: Fix TopicId conversion
                    TopicName = "", // TODO: Batch fetch topic names
                    QuestionType = q.QuestionType,
                    QuestionText = q.Question1,
                    Explanation = q.Explanation,
                    IsActive = q.IsActive ?? true,
                    CreatedAt = q.CreatedAt,
                    UpdatedAt = q.UpdatedAt,
                    Options = q.Options.Select(o => new OptionDto
                    {
                        Id = o.OptionId,
                        Answer = o.Answer,
                        IsCorrect = o.IsCorrect ?? false
                    }).ToList()
                }).ToList();

                return ApiResponse<List<QuestionDto>>.SuccessResult(result, $"Lấy danh sách {result.Count} câu hỏi thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<QuestionDto>>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing question
        /// </summary>
        public async Task<ApiResponse<QuestionDto>> UpdateQuestionAsync(Guid id, UpdateQuestionDto updateQuestionDto)
        {
            try
            {
                // Validate at least one correct answer
                if (!updateQuestionDto.Options.Any(o => o.IsCorrect))
                {
                    return ApiResponse<QuestionDto>.ErrorResult("Phải có ít nhất một đáp án đúng");
                }

                var question = await _unitOfWork.Questions.GetByIdAsync(id);

                if (question == null)
                {
                    return ApiResponse<QuestionDto>.ErrorResult($"Không tìm thấy câu hỏi với ID: {id}");
                }

                // Validate Topic exists
                var topicResponse = await _slideServiceClient.GetTopicByIdAsync(updateQuestionDto.TopicId);
                if (topicResponse?.Success != true || topicResponse.Data == null)
                {
                    return ApiResponse<QuestionDto>.ErrorResult("Topic không tồn tại");
                }

                // Update Question
                question.QuestionType = updateQuestionDto.QuestionType;
                question.Question1 = updateQuestionDto.QuestionText;
                question.Explanation = updateQuestionDto.Explanation;
                question.IsActive = updateQuestionDto.IsActive;
                question.UpdatedAt = DateTime.UtcNow;
                question.UpdatedBy = null; // TODO: Get from authenticated user

                // Remove old options
                foreach (var option in question.Options.ToList())
                {
                    await _unitOfWork.Options.RemoveAsync(option);
                }

                // Add new options
                var newOptions = updateQuestionDto.Options.Select(opt => new Option
                {
                    OptionId = Guid.NewGuid(),
                    QuestionId = question.QuestionId,
                    Answer = opt.Answer,
                    IsCorrect = opt.IsCorrect,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = null
                }).ToList();

                question.Options = newOptions;

                await _unitOfWork.Questions.UpdateAsync(question);
                await _unitOfWork.SaveChangesAsync();

                var result = new QuestionDto
                {
                    Id = question.QuestionId,
                    TopicId = updateQuestionDto.TopicId,
                    TopicName = topicResponse.Data.Name,
                    QuestionType = question.QuestionType,
                    QuestionText = question.Question1,
                    Explanation = question.Explanation,
                    IsActive = question.IsActive ?? true,
                    UpdatedAt = question.UpdatedAt,
                    Options = newOptions.Select(o => new OptionDto
                    {
                        Id = o.OptionId,
                        Answer = o.Answer,
                        IsCorrect = o.IsCorrect ?? false
                    }).ToList()
                };

                return ApiResponse<QuestionDto>.SuccessResult(result, "Cập nhật câu hỏi thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<QuestionDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a question (soft delete)
        /// </summary>
        public async Task<ApiResponse<bool>> DeleteQuestionAsync(Guid id)
        {
            try
            {
                var question = await _unitOfWork.Questions.GetByIdAsync(id);

                if (question == null)
                {
                    return ApiResponse<bool>.ErrorResult($"Không tìm thấy câu hỏi với ID: {id}");
                }

                // Soft delete
                question.IsActive = false;
                question.UpdatedAt = DateTime.UtcNow;
                question.UpdatedBy = null; // TODO: Get from authenticated user

                await _unitOfWork.Questions.UpdateAsync(question);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Xóa câu hỏi thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }
    }
}
