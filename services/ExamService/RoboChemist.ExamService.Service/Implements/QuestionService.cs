using RoboChemist.ExamService.Repository.Interfaces;
using RoboChemist.ExamService.Service.Interfaces;
using RoboChemist.ExamService.Service.HttpClients;
using RoboChemist.ExamService.Model.Models;
using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.ExamServiceDTOs.QuestionDTOs;
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
        private readonly ISlidesServiceHttpClient _slideServiceClient;
        private readonly IAuthServiceClient _authServiceClient;

        public QuestionService(
            IUnitOfWork unitOfWork, 
            ISlidesServiceHttpClient slideServiceClient,
            IAuthServiceClient authServiceClient)
        {
            _unitOfWork = unitOfWork;
            _slideServiceClient = slideServiceClient;
            _authServiceClient = authServiceClient;
        }

        /// <summary>
        /// Create a new question with options
        /// </summary>
        public async Task<ApiResponse<QuestionResponseDto>> CreateQuestionAsync(CreateQuestionDto createQuestionDto)
        {
            try
            {
                // Validate at least one correct answer
                if (!createQuestionDto.Options.Any(o => o.IsCorrect))
                {
                    return ApiResponse<QuestionResponseDto>.ErrorResult("Phải có ít nhất một đáp án đúng");
                }

                // Get current authenticated user
                var currentUser = await _authServiceClient.GetCurrentUserAsync();

                // Validate Topic existence by calling Slide Service
                var topicResponse = await _slideServiceClient.GetTopicByIdAsync(createQuestionDto.TopicId);

                if (topicResponse is null)
                {
                    return ApiResponse<QuestionResponseDto>.ErrorResult("Không nhận được phản hồi từ Slide Service");
                }

                if (!topicResponse.Success || topicResponse.Data is null)
                {
                    var msg = string.IsNullOrWhiteSpace(topicResponse.Message)
                        ? $"Topic không tồn tại: {createQuestionDto.TopicId}"
                        : topicResponse.Message;
                    return ApiResponse<QuestionResponseDto>.ErrorResult(msg);
                }

                // Create Question entity
                var question = new Question
                {
                    QuestionId = Guid.NewGuid(),
                    TopicId = createQuestionDto.TopicId,
                    QuestionType = createQuestionDto.QuestionType,
                    QuestionText = createQuestionDto.QuestionText,
                    Explanation = createQuestionDto.Explanation,
                    Level = createQuestionDto.Level,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = currentUser?.Id 
                };

                // Create Options
                var options = createQuestionDto.Options.Select(opt => new Option
                {
                    OptionId = Guid.NewGuid(),
                    QuestionId = question.QuestionId,
                    Answer = opt.Answer,
                    IsCorrect = opt.IsCorrect,
                    CreatedAt = DateTime.Now,
                    CreatedBy = currentUser?.Id
                }).ToList();

                question.Options = options;

                // Save to database
                await _unitOfWork.Questions.CreateAsync(question);

                // Return DTO
                var result = new QuestionResponseDto
                {
                    QuestionId = question.QuestionId,
                    TopicId = createQuestionDto.TopicId,
                    TopicName = topicResponse.Data.Name,
                    QuestionType = question.QuestionType,
                    QuestionText = question.QuestionText,
                    Explanation = question.Explanation,
                    Level = question.Level,
                    Status = question.IsActive == true ? "1" : "0",
                    CreatedAt = question.CreatedAt,
                    CreatedBy = question.CreatedBy,
                    Options = options.Select(o => new OptionResponseDto
                    {
                        OptionId = o.OptionId,
                        Answer = o.Answer,
                        IsCorrect = o.IsCorrect ?? false,
                        CreatedAt = o.CreatedAt,
                        CreatedBy = o.CreatedBy
                    }).ToList()
                };

                return ApiResponse<QuestionResponseDto>.SuccessResult(result, "Tạo câu hỏi thành công");
            }
            catch (HttpRequestException ex)
            {
                return ApiResponse<QuestionResponseDto>.ErrorResult($"Lỗi kết nối tới Slide Service: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse<QuestionResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a question by ID
        /// </summary>
        public async Task<ApiResponse<QuestionResponseDto>> GetQuestionByIdAsync(Guid id)
        {
            try
            {
                var question = await _unitOfWork.Questions.GetByIdAsync(id);

                if (question == null)
                {
                    return ApiResponse<QuestionResponseDto>.ErrorResult($"Không tìm thấy câu hỏi với ID: {id}");
                }

                var topicResponse = await _slideServiceClient.GetTopicByIdAsync(question.TopicId ?? Guid.Empty);
                var topicName = topicResponse?.Success == true ? topicResponse.Data?.Name ?? "" : "";

                var result = new QuestionResponseDto
                {
                    QuestionId = question.QuestionId,
                    TopicId = question.TopicId ?? Guid.Empty,
                    TopicName = topicName,
                    QuestionType = question.QuestionType,
                    QuestionText = question.QuestionText,
                    Explanation = question.Explanation,
                    Level = question.Level,
                    Status = question.IsActive == true ? "1" : "0",
                    CreatedAt = question.CreatedAt,
                    CreatedBy = question.CreatedBy,
                    Options = question.Options.Select(o => new OptionResponseDto
                    {
                        OptionId = o.OptionId,
                        Answer = o.Answer,
                        IsCorrect = o.IsCorrect ?? false,
                        CreatedAt = o.CreatedAt,
                        CreatedBy = o.CreatedBy
                    }).ToList()
                };

                return ApiResponse<QuestionResponseDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<QuestionResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all questions, optionally filtered by topic and search term
        /// </summary>
        public async Task<ApiResponse<List<QuestionResponseDto>>> GetQuestionsAsync(Guid? topicId = null, string? search = null, string? level = null)
        {
            try
            {
                var questions = await _unitOfWork.Questions.GetQuestionsWithFiltersAsync(topicId, search, level);

                var topicIds = questions
                    .Where(q => q.TopicId.HasValue)
                    .Select(q => q.TopicId!.Value)
                    .Distinct()
                    .ToList();

                var topicsDict = await _slideServiceClient.GetTopicsByIdsAsync(topicIds);

                var result = questions.Select(q =>
                {
                    var qTopicId = q.TopicId ?? Guid.Empty;
                    var topicName = topicsDict.TryGetValue(qTopicId, out var topic)
                        ? topic.Name
                        : "";

                    return new QuestionResponseDto
                    {
                        QuestionId = q.QuestionId,
                        TopicId = qTopicId,
                        TopicName = topicName,
                        QuestionType = q.QuestionType,
                        QuestionText = q.QuestionText,
                        Explanation = q.Explanation,
                        Level = q.Level,
                        Status = q.IsActive == true ? "1" : "0",
                        CreatedAt = q.CreatedAt,
                        CreatedBy = q.CreatedBy,
                        Options = q.Options.Select(o => new OptionResponseDto
                        {
                            OptionId = o.OptionId,
                            Answer = o.Answer,
                            IsCorrect = o.IsCorrect ?? false,
                            CreatedAt = o.CreatedAt,
                            CreatedBy = o.CreatedBy
                        }).ToList()
                    };
                }).ToList();

                return ApiResponse<List<QuestionResponseDto>>.SuccessResult(result, $"Lấy danh sách {result.Count} câu hỏi thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<QuestionResponseDto>>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing question
        /// </summary>
        public async Task<ApiResponse<QuestionResponseDto>> UpdateQuestionAsync(Guid id, UpdateQuestionDto updateQuestionDto)
        {
            try
            {
                // Validate at least one correct answer
                if (!updateQuestionDto.Options.Any(o => o.IsCorrect))
                {
                    return ApiResponse<QuestionResponseDto>.ErrorResult("Phải có ít nhất một đáp án đúng");
                }

                var question = await _unitOfWork.Questions.GetQuestionsByIdsAsync(id);

                if (question == null)
                {
                    return ApiResponse<QuestionResponseDto>.ErrorResult($"Không tìm thấy câu hỏi với ID: {id}");
                }

                // Validate Topic exists
                var topicResponse = await _slideServiceClient.GetTopicByIdAsync(updateQuestionDto.TopicId);
                if (topicResponse?.Success != true || topicResponse.Data == null)
                {
                    return ApiResponse<QuestionResponseDto>.ErrorResult("Topic không tồn tại");
                }

                // Update Question fields
                question.QuestionType = updateQuestionDto.QuestionType;
                question.QuestionText = updateQuestionDto.QuestionText;
                question.Explanation = updateQuestionDto.Explanation;
                question.Level = updateQuestionDto.Level;
                question.IsActive = updateQuestionDto.Status == "1";

                await _unitOfWork.Questions.UpdateAsync(question);

                // Xóa options cũ
                foreach (var option in question.Options.ToList())
                {
                    await _unitOfWork.Options.RemoveAsync(option);
                }

                // Tạo options mới
                var newOptions = new List<Option>();
                foreach (var opt in updateQuestionDto.Options)
                {
                    var newOption = new Option
                    {
                        OptionId = Guid.NewGuid(),
                        QuestionId = question.QuestionId,
                        Answer = opt.Answer,
                        IsCorrect = opt.IsCorrect,
                        CreatedAt = DateTime.Now
                    };
                    await _unitOfWork.Options.CreateAsync(newOption);
                    newOptions.Add(newOption);
                }

                var result = new QuestionResponseDto
                {
                    QuestionId = question.QuestionId,
                    TopicId = updateQuestionDto.TopicId,
                    TopicName = topicResponse.Data.Name,
                    QuestionType = question.QuestionType,
                    QuestionText = question.QuestionText,
                    Explanation = question.Explanation,
                    Level = question.Level,
                    Status = question.IsActive == true ? "1" : "0",
                    CreatedAt = question.CreatedAt,
                    CreatedBy = question.CreatedBy,
                    Options = newOptions.Select(o => new OptionResponseDto
                    {
                        OptionId = o.OptionId,
                        Answer = o.Answer,
                        IsCorrect = o.IsCorrect ?? false,
                        CreatedAt = o.CreatedAt,
                        CreatedBy = o.CreatedBy
                    }).ToList()
                };

                return ApiResponse<QuestionResponseDto>.SuccessResult(result, "Cập nhật câu hỏi thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<QuestionResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
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

                question.IsActive = false;

                await _unitOfWork.Questions.UpdateAsync(question);

                return ApiResponse<bool>.SuccessResult(true, "Xóa câu hỏi thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Bulk create questions for a topic
        /// </summary>
        public async Task<ApiResponse<BulkCreateQuestionsResponseDto>> BulkCreateQuestionsAsync(BulkCreateQuestionsDto bulkCreateDto)
        {
            try
            {
                // Get current authenticated user
                var currentUser = await _authServiceClient.GetCurrentUserAsync();

                var topicResponse = await _slideServiceClient.GetTopicByIdAsync(bulkCreateDto.TopicId);

                if (topicResponse is null)
                {
                    return ApiResponse<BulkCreateQuestionsResponseDto>.ErrorResult("Không nhận được phản hồi từ Slide Service");
                }

                if (!topicResponse.Success || topicResponse.Data is null)
                {
                    var msg = string.IsNullOrWhiteSpace(topicResponse.Message)
                        ? $"Topic không tồn tại: {bulkCreateDto.TopicId}"
                        : topicResponse.Message;
                    return ApiResponse<BulkCreateQuestionsResponseDto>.ErrorResult(msg);
                }

                var createdQuestionIds = new List<Guid>();

                // Create all questions
                foreach (var questionItem in bulkCreateDto.Questions)
                {
                    // Validate at least one correct answer
                    if (!questionItem.Options.Any(o => o.IsCorrect))
                    {
                        return ApiResponse<BulkCreateQuestionsResponseDto>.ErrorResult(
                            $"Câu hỏi '{questionItem.QuestionText}' phải có ít nhất một đáp án đúng");
                    }

                    var createdAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                    
                    var question = new Question
                    {
                        QuestionId = Guid.NewGuid(),
                        TopicId = bulkCreateDto.TopicId,
                        QuestionType = questionItem.QuestionType,
                        QuestionText = questionItem.QuestionText,
                        Level = questionItem.Level,
                        Explanation = questionItem.Explanation,
                        IsActive = true,
                        CreatedAt = createdAt,
                        CreatedBy = currentUser?.Id
                    };

                    var options = questionItem.Options.Select(opt => new Option
                    {
                        OptionId = Guid.NewGuid(),
                        QuestionId = question.QuestionId,
                        Answer = opt.Answer,
                        IsCorrect = opt.IsCorrect,
                        CreatedAt = createdAt,
                        CreatedBy = currentUser?.Id
                    }).ToList();

                    question.Options = options;

                    await _unitOfWork.Questions.CreateAsync(question);
                    createdQuestionIds.Add(question.QuestionId);
                }

                var result = new BulkCreateQuestionsResponseDto
                {
                    TotalCreated = createdQuestionIds.Count,
                    TopicId = bulkCreateDto.TopicId,
                    QuestionType = topicResponse.Data.Name ?? "",
                    CreatedQuestionIds = createdQuestionIds,
                    Message = $"Đã tạo {createdQuestionIds.Count} câu hỏi thành công cho Topic: {topicResponse.Data.Name}"
                };

                return ApiResponse<BulkCreateQuestionsResponseDto>.SuccessResult(result);
            }
            catch (HttpRequestException ex)
            {
                var errorMsg = ex.InnerException != null 
                    ? $"{ex.Message} Chi tiết: {ex.InnerException.Message}" 
                    : ex.Message;
                return ApiResponse<BulkCreateQuestionsResponseDto>.ErrorResult($"Lỗi kết nối tới Slide Service: {errorMsg}");
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null 
                    ? $"{ex.Message} Chi tiết: {ex.InnerException.Message}" 
                    : ex.Message;
                return ApiResponse<BulkCreateQuestionsResponseDto>.ErrorResult($"Lỗi hệ thống: {errorMsg}");
            }
        }

        public async Task<ApiResponse<QuestionCountResponseDto>> CountQuestionsByFiltersAsync(Guid topicId, string questionType, string? level = null)
        {
            try
            {
                var count = await _unitOfWork.Questions.CountQuestionsByFiltersAsync(
                    topicId,
                    questionType,
                    level,
                    isActive: true
                );

                var response = new QuestionCountResponseDto
                {
                    TopicId = topicId,
                    QuestionType = questionType,
                    Level = level,
                    AvailableCount = count
                };

                return ApiResponse<QuestionCountResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<QuestionCountResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }
    }
}
