using System.Text.Json;
using Microsoft.AspNetCore.Http;
using RoboChemist.ExamService.Model.Models;
using RoboChemist.ExamService.Repository.Interfaces;
using RoboChemist.ExamService.Service.Interfaces;
using RoboChemist.ExamService.Service.HttpClients;
using RoboChemist.Shared.Common.Constants;
using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.ExamServiceDTOs.QuestionDTOs;

namespace RoboChemist.ExamService.Service.Implements
{
    /// <summary>
    /// Implementation của Question Service - Quản lý câu hỏi
    /// </summary>
    public class QuestionService : IQuestionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISlidesServiceHttpClient _slidesServiceHttpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public QuestionService(
            IUnitOfWork unitOfWork, 
            ISlidesServiceHttpClient slidesServiceHttpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _slidesServiceHttpClient = slidesServiceHttpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Tạo mới Question với các Options (VALIDATION: TopicId -> Correct answer -> Question type rules)
        /// </summary>
        public async Task<ApiResponse<QuestionResponseDto>> CreateQuestionAsync(CreateQuestionDto createQuestionDto)
        {
            try
            {
                // Lấy userId từ JWT token trong HttpContext
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub") 
                    ?? _httpContextAccessor.HttpContext?.User?.FindFirst("userId");
                
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return ApiResponse<QuestionResponseDto>.ErrorResult("Không xác thực được user từ token");
                }

                // Lấy authToken từ Request header
                var authToken = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"]
                    .ToString()
                    .Replace("Bearer ", "");

                //Kiểm tra TopicId có tồn tại trong SlidesService không
                // Gọi API GetTopicById của SlidesService
                var isTopicExists = await _slidesServiceHttpClient.GetTopicByIdAsync(createQuestionDto.TopicId, authToken);
                if (!isTopicExists)
                {
                    return ApiResponse<QuestionResponseDto>.ErrorResult($"TopicId {createQuestionDto.TopicId} không tồn tại trong hệ thống");
                }

                // Phải có ít nhất 1 đáp án đúng (trừ Essay)
                if (createQuestionDto.QuestionType != RoboChemistConstants.QUESTION_TYPE_ESSAY && 
                    !createQuestionDto.Options.Any(o => o.IsCorrect))
                {
                    return ApiResponse<QuestionResponseDto>.ErrorResult("Phải có ít nhất một đáp án đúng");
                }

                // Với TrueFalse thì chỉ được có đúng 2 options
                if (createQuestionDto.QuestionType == RoboChemistConstants.QUESTION_TYPE_TRUE_FALSE && 
                    createQuestionDto.Options.Count != 2)
                {
                    return ApiResponse<QuestionResponseDto>.ErrorResult("Câu hỏi True/False phải có đúng 2 đáp án");
                }

                // Với MultipleChoice thì phải có từ 2-6 options
                if (createQuestionDto.QuestionType == RoboChemistConstants.QUESTION_TYPE_MULTIPLE_CHOICE)
                {
                    if (createQuestionDto.Options.Count < RoboChemistConstants.MIN_OPTIONS_COUNT ||
                        createQuestionDto.Options.Count > RoboChemistConstants.MAX_OPTIONS_COUNT)
                    {
                        return ApiResponse<QuestionResponseDto>.ErrorResult(
                            $"Câu hỏi Multiple Choice phải có từ {RoboChemistConstants.MIN_OPTIONS_COUNT} đến {RoboChemistConstants.MAX_OPTIONS_COUNT} đáp án");
                    }
                }

                // Với Essay thì không cần options (hoặc có thể có options làm gợi ý)
                if (createQuestionDto.QuestionType == RoboChemistConstants.QUESTION_TYPE_ESSAY && 
                    createQuestionDto.Options.Count > 0)
                {
                    return ApiResponse<QuestionResponseDto>.ErrorResult("Câu hỏi Tự luận (Essay) không cần đáp án");
                }

                // Tạo Question entity
                var question = new Question
                {
                    QuestionId = Guid.NewGuid(),
                    TopicId = createQuestionDto.TopicId,
                    QuestionType = createQuestionDto.QuestionType,
                    QuestionText = createQuestionDto.QuestionText,  
                    Explanation = createQuestionDto.Explanation,
                    IsActive = true, // bool: true = active
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId
                };

                // Tạo Options
                var options = createQuestionDto.Options.Select(opt => new Option
                {
                    OptionId = Guid.NewGuid(),
                    QuestionId = question.QuestionId,
                    Answer = opt.Answer,
                    IsCorrect = opt.IsCorrect, // bool: true/false
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId
                }).ToList();

                question.Options = options;

                // Lưu vào database
                await _unitOfWork.Questions.CreateAsync(question);

                // API COMPOSITION Lấy TopicName từ SlidesService
                var topicDto = await _slidesServiceHttpClient.GetTopicAsync(question.TopicId, authToken);

                var response = new QuestionResponseDto
                {
                    QuestionId = question.QuestionId,
                    TopicId = question.TopicId,
                    TopicName = topicDto?.Name, // API Composition
                    QuestionType = question.QuestionType,
                    QuestionText = question.QuestionText,
                    Explanation = question.Explanation,
                    Status = question.IsActive == true ? RoboChemistConstants.QUESTION_STATUS_ACTIVE : RoboChemistConstants.QUESTION_STATUS_INACTIVE,
                    CreatedAt = question.CreatedAt,
                    CreatedBy = question.CreatedBy,
                    Options = options.Select(o => new OptionResponseDto
                    {
                        OptionId = o.OptionId,
                        Answer = o.Answer,
                        IsCorrect = o.IsCorrect ?? false, // bool: true/false
                        CreatedAt = o.CreatedAt,
                        CreatedBy = o.CreatedBy
                    }).ToList()
                };

                return ApiResponse<QuestionResponseDto>.SuccessResult(response, "Tạo câu hỏi thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<QuestionResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin Question theo ID (chỉ đọc từ model, không gọi external API)
        /// </summary>
        public async Task<ApiResponse<QuestionResponseDto>> GetQuestionByIdAsync(Guid id)
        {
            try
            {
                // Lấy authToken từ Request header (cần cho API Composition)
                var authToken = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"]
                    .ToString()
                    .Replace("Bearer ", "");

                // Lấy Question từ database
                var question = await _unitOfWork.Questions.GetByIdAsync(id);

                if (question == null)
                {
                    return ApiResponse<QuestionResponseDto>.ErrorResult($"Không tìm thấy câu hỏi với ID: {id}");
                }

                // Load Options cho Question
                var allOptions = await _unitOfWork.Options.GetAllAsync();
                question.Options = allOptions.Where(o => o.QuestionId == id).ToList();

                // [API COMPOSITION] Lấy TopicName từ SlidesService với authToken
                var topicDto = await _slidesServiceHttpClient.GetTopicAsync(question.TopicId, authToken);

                // Map sang ResponseDto với TopicName enriched
                var response = new QuestionResponseDto
                {
                    QuestionId = question.QuestionId,
                    TopicId = question.TopicId,
                    TopicName = topicDto?.Name, // API Composition: Enrich từ SlidesService
                    QuestionType = question.QuestionType,
                    QuestionText = question.QuestionText,
                    Explanation = question.Explanation,
                    Status = question.IsActive == true ? RoboChemistConstants.QUESTION_STATUS_ACTIVE : RoboChemistConstants.QUESTION_STATUS_INACTIVE,
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

                return ApiResponse<QuestionResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<QuestionResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách Questions với filter (query từ repository, không filter trên service)
        /// </summary>
        public async Task<ApiResponse<List<QuestionResponseDto>>> GetQuestionsAsync(Guid? topicId = null, string? status = null)
        {
            try
            {
                // Lấy authToken từ Request header (cần cho API Composition)
                var authToken = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"]
                    .ToString()
                    .Replace("Bearer ", "");

                List<Question> allQuestions;

                // Validate status nếu có
                bool? isActiveFilter = null;
                if (!string.IsNullOrEmpty(status))
                {
                    if (status != RoboChemistConstants.QUESTION_STATUS_ACTIVE && 
                        status != RoboChemistConstants.QUESTION_STATUS_INACTIVE)
                    {
                        return ApiResponse<List<QuestionResponseDto>>.ErrorResult(
                            $"Status không hợp lệ. Cho phép: {RoboChemistConstants.QUESTION_STATUS_ACTIVE} (Active) hoặc {RoboChemistConstants.QUESTION_STATUS_INACTIVE} (Inactive)");
                    }
                    isActiveFilter = status == RoboChemistConstants.QUESTION_STATUS_ACTIVE;
                }

                // Gọi repository method phù hợp dựa trên filter parameters
                if (topicId.HasValue && isActiveFilter.HasValue)
                {
                    // Filter cả TopicId và Status
                    allQuestions = await _unitOfWork.Questions.GetByTopicIdAndStatusAsync(topicId.Value, isActiveFilter.Value);
                }
                else if (topicId.HasValue)
                {
                    // Chỉ filter TopicId
                    allQuestions = await _unitOfWork.Questions.GetByTopicIdAsync(topicId.Value);
                }
                else if (isActiveFilter.HasValue)
                {
                    // Chỉ filter Status
                    allQuestions = await _unitOfWork.Questions.GetByStatusAsync(isActiveFilter.Value);
                }
                else
                {
                    // Không filter gì, lấy tất cả
                    allQuestions = await _unitOfWork.Questions.GetAllAsync();
                    allQuestions = allQuestions.OrderByDescending(q => q.CreatedAt).ToList();
                }

                // Load Options cho tất cả questions
                var allOptions = await _unitOfWork.Options.GetAllAsync();

                // [API COMPOSITION] Lấy unique TopicIds và batch call SlidesService với authToken
                var uniqueTopicIds = allQuestions.Select(q => q.TopicId).Distinct().ToList();
                var topicDictionary = new Dictionary<Guid, string>();
                
                foreach (var tid in uniqueTopicIds)
                {
                    var topicDto = await _slidesServiceHttpClient.GetTopicAsync(tid, authToken);
                    if (topicDto != null)
                    {
                        topicDictionary[tid] = topicDto.Name;
                    }
                }

                var response = new List<QuestionResponseDto>();
                foreach (var question in allQuestions)
                {
                    // Load Options
                    question.Options = allOptions.Where(o => o.QuestionId == question.QuestionId).ToList();

                    // Lấy TopicName từ dictionary
                    topicDictionary.TryGetValue(question.TopicId, out var topicName);

                    // Map sang ResponseDto với TopicName enriched
                    response.Add(new QuestionResponseDto
                    {
                        QuestionId = question.QuestionId,
                        TopicId = question.TopicId,
                        TopicName = topicName, // API Composition: Enrich từ SlidesService
                        QuestionType = question.QuestionType,
                        QuestionText = question.QuestionText,  
                        Explanation = question.Explanation,
                        Status = question.IsActive == true ? RoboChemistConstants.QUESTION_STATUS_ACTIVE : RoboChemistConstants.QUESTION_STATUS_INACTIVE,
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
                    });
                }

                return ApiResponse<List<QuestionResponseDto>>.SuccessResult(response, $"Tìm thấy {response.Count} câu hỏi");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<QuestionResponseDto>>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật Question
        /// </summary>
        public async Task<ApiResponse<QuestionResponseDto>> UpdateQuestionAsync(Guid id, UpdateQuestionDto updateQuestionDto)
        {
            try
            {
                // Lấy userId từ JWT token trong HttpContext
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub") 
                    ?? _httpContextAccessor.HttpContext?.User?.FindFirst("userId");
                
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return ApiResponse<QuestionResponseDto>.ErrorResult("Không xác thực được user từ token");
                }

                // Lấy authToken từ Request header
                var authToken = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"]
                    .ToString()
                    .Replace("Bearer ", "");

                // Kiểm tra Question có tồn tại không
                var question = await _unitOfWork.Questions.GetByIdAsync(id);
                if (question == null)
                {
                    return ApiResponse<QuestionResponseDto>.ErrorResult($"Không tìm thấy câu hỏi với ID: {id}");
                }

                // Kiểm tra TopicId có tồn tại trong SlidesService không
                // Gọi API GetTopicById của SlidesService
                var isTopicExists = await _slidesServiceHttpClient.GetTopicByIdAsync(updateQuestionDto.TopicId, authToken);
                if (!isTopicExists)
                {
                    return ApiResponse<QuestionResponseDto>.ErrorResult($"TopicId {updateQuestionDto.TopicId} không tồn tại trong hệ thống");
                }

                // Phải có ít nhất 1 đáp án đúng (trừ Essay)
                if (updateQuestionDto.QuestionType != RoboChemistConstants.QUESTION_TYPE_ESSAY && 
                    !updateQuestionDto.Options.Any(o => o.IsCorrect))
                {
                    return ApiResponse<QuestionResponseDto>.ErrorResult("Phải có ít nhất một đáp án đúng");
                }

                // Với TrueFalse thì chỉ được có đúng 2 options
                if (updateQuestionDto.QuestionType == RoboChemistConstants.QUESTION_TYPE_TRUE_FALSE && 
                    updateQuestionDto.Options.Count != 2)
                {
                    return ApiResponse<QuestionResponseDto>.ErrorResult("Câu hỏi True/False phải có đúng 2 đáp án");
                }

                // Với MultipleChoice thì phải có từ 2-6 options
                if (updateQuestionDto.QuestionType == RoboChemistConstants.QUESTION_TYPE_MULTIPLE_CHOICE)
                {
                    if (updateQuestionDto.Options.Count < RoboChemistConstants.MIN_OPTIONS_COUNT ||
                        updateQuestionDto.Options.Count > RoboChemistConstants.MAX_OPTIONS_COUNT)
                    {
                        return ApiResponse<QuestionResponseDto>.ErrorResult(
                            $"Câu hỏi Multiple Choice phải có từ {RoboChemistConstants.MIN_OPTIONS_COUNT} đến {RoboChemistConstants.MAX_OPTIONS_COUNT} đáp án");
                    }
                }

                // Với Essay thì không cần options
                if (updateQuestionDto.QuestionType == RoboChemistConstants.QUESTION_TYPE_ESSAY && 
                    updateQuestionDto.Options.Count > 0)
                {
                    return ApiResponse<QuestionResponseDto>.ErrorResult("Câu hỏi Tự luận (Essay) không cần đáp án");
                }

                // Cập nhật Question
                question.TopicId = updateQuestionDto.TopicId;
                question.QuestionType = updateQuestionDto.QuestionType;
                question.QuestionText = updateQuestionDto.QuestionText;  
                question.Explanation = updateQuestionDto.Explanation;
                question.IsActive = updateQuestionDto.Status == RoboChemistConstants.QUESTION_STATUS_ACTIVE; // Convert string "1"/"0" -> bool

                // Xóa tất cả Options cũ
                var allOptions = await _unitOfWork.Options.GetAllAsync();
                var oldOptions = allOptions.Where(o => o.QuestionId == id).ToList();
                foreach (var oldOption in oldOptions)
                {
                    await _unitOfWork.Options.RemoveAsync(oldOption);
                }

                // Tạo Options mới
                var newOptions = updateQuestionDto.Options.Select(opt => new Option
                {
                    OptionId = Guid.NewGuid(),
                    QuestionId = question.QuestionId,
                    Answer = opt.Answer,
                    IsCorrect = opt.IsCorrect,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId
                }).ToList();

                question.Options = newOptions;

                await _unitOfWork.Questions.UpdateAsync(question);

                // [API COMPOSITION] Lấy TopicName từ SlidesService
                var topicDto = await _slidesServiceHttpClient.GetTopicAsync(question.TopicId, authToken);

                // Map sang ResponseDto với TopicName enriched
                var response = new QuestionResponseDto
                {
                    QuestionId = question.QuestionId,
                    TopicId = question.TopicId,
                    TopicName = topicDto?.Name, // API Composition: Enrich từ SlidesService
                    QuestionType = question.QuestionType,
                    QuestionText = question.QuestionText,  
                    Explanation = question.Explanation,
                    Status = question.IsActive == true ? RoboChemistConstants.QUESTION_STATUS_ACTIVE : RoboChemistConstants.QUESTION_STATUS_INACTIVE,
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

                return ApiResponse<QuestionResponseDto>.SuccessResult(response, "Cập nhật câu hỏi thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<QuestionResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa Question (soft delete - set IsActive = false)
        /// </summary>
        public async Task<ApiResponse<bool>> DeleteQuestionAsync(Guid id)
        {
            try
            {
                // Lấy userId từ JWT token trong HttpContext
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub") 
                    ?? _httpContextAccessor.HttpContext?.User?.FindFirst("userId");
                
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return ApiResponse<bool>.ErrorResult("Không xác thực được user từ token");
                }

                // VALIDATION: Kiểm tra Question có tồn tại không
                var question = await _unitOfWork.Questions.GetByIdAsync(id);
                if (question == null)
                {
                    return ApiResponse<bool>.ErrorResult($"Không tìm thấy câu hỏi với ID: {id}");
                }

                // Soft delete - set IsActive = false
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
        /// [TEMP - FOR SEEDING] Tạo nhiều Questions cùng 1 Topic từ data có sẵn
        /// </summary>
        public async Task<ApiResponse<BulkCreateQuestionsResponseDto>> BulkCreateQuestionsAsync(BulkCreateQuestionsDto bulkCreateDto)
        {
            try
            {
                // Lấy userId từ JWT token trong HttpContext
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub") 
                    ?? _httpContextAccessor.HttpContext?.User?.FindFirst("userId");
                
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return ApiResponse<BulkCreateQuestionsResponseDto>.ErrorResult("Không xác thực được user từ token");
                }

                // Lấy authToken từ Request header
                var authToken = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"]
                    .ToString()
                    .Replace("Bearer ", "");

                // Validate Topic tồn tại trong SlidesService (1 lần thôi cho cả batch)
                var isTopicExists = await _slidesServiceHttpClient.GetTopicByIdAsync(bulkCreateDto.TopicId, authToken);
                if (!isTopicExists)
                {
                    return ApiResponse<BulkCreateQuestionsResponseDto>.ErrorResult(
                        $"TopicId {bulkCreateDto.TopicId} không tồn tại trong hệ thống");
                }

                var createdQuestionIds = new List<Guid>();
                var questionTypes = new HashSet<string>();

                // Tạo từng Question từ list
                foreach (var questionItem in bulkCreateDto.Questions)
                {
                    // Validate từng question
                    if (questionItem.QuestionType != RoboChemistConstants.QUESTION_TYPE_ESSAY 
                        && !questionItem.Options.Any(o => o.IsCorrect))
                    {
                        return ApiResponse<BulkCreateQuestionsResponseDto>.ErrorResult(
                            $"Câu hỏi '{questionItem.QuestionText}' phải có ít nhất một đáp án đúng");
                    }

                    if (questionItem.QuestionType == RoboChemistConstants.QUESTION_TYPE_TRUE_FALSE 
                        && questionItem.Options.Count != 2)
                    {
                        return ApiResponse<BulkCreateQuestionsResponseDto>.ErrorResult(
                            $"Câu hỏi '{questionItem.QuestionText}' True/False phải có đúng 2 đáp án");
                    }

                    if (questionItem.QuestionType == RoboChemistConstants.QUESTION_TYPE_MULTIPLE_CHOICE)
                    {
                        if (questionItem.Options.Count < RoboChemistConstants.MIN_OPTIONS_COUNT ||
                            questionItem.Options.Count > RoboChemistConstants.MAX_OPTIONS_COUNT)
                        {
                            return ApiResponse<BulkCreateQuestionsResponseDto>.ErrorResult(
                                $"Câu hỏi '{questionItem.QuestionText}' Multiple Choice phải có từ {RoboChemistConstants.MIN_OPTIONS_COUNT} đến {RoboChemistConstants.MAX_OPTIONS_COUNT} đáp án");
                        }
                    }

                    if (questionItem.QuestionType == RoboChemistConstants.QUESTION_TYPE_ESSAY 
                        && questionItem.Options.Count > 0)
                    {
                        return ApiResponse<BulkCreateQuestionsResponseDto>.ErrorResult(
                            $"Câu hỏi '{questionItem.QuestionText}' Essay không cần đáp án");
                    }

                    // Tạo Question entity
                    var question = new Question
                    {
                        QuestionId = Guid.NewGuid(),
                        TopicId = bulkCreateDto.TopicId,
                        QuestionType = questionItem.QuestionType,
                        QuestionText = questionItem.QuestionText,
                        Explanation = questionItem.Explanation,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = userId
                    };

                    await _unitOfWork.Questions.CreateAsync(question);
                    createdQuestionIds.Add(question.QuestionId);
                    questionTypes.Add(questionItem.QuestionType);

                    // Tạo Options cho Question
                    foreach (var optionDto in questionItem.Options)
                    {
                        var option = new Option
                        {
                            OptionId = Guid.NewGuid(),
                            QuestionId = question.QuestionId,
                            Answer = optionDto.Answer,
                            IsCorrect = optionDto.IsCorrect,
                            CreatedAt = DateTime.Now,
                            CreatedBy = userId
                        };

                        await _unitOfWork.Options.CreateAsync(option);
                    }
                }

                var response = new BulkCreateQuestionsResponseDto
                {
                    TotalCreated = bulkCreateDto.Questions.Count,
                    TopicId = bulkCreateDto.TopicId,
                    QuestionType = string.Join(", ", questionTypes), // Hiển thị tất cả types đã tạo
                    CreatedQuestionIds = createdQuestionIds,
                    Message = $"Đã tạo thành công {bulkCreateDto.Questions.Count} câu hỏi cho Topic {bulkCreateDto.TopicId}"
                };

                return ApiResponse<BulkCreateQuestionsResponseDto>.SuccessResult(
                    response, 
                    $"Bulk create thành công {bulkCreateDto.Questions.Count} câu hỏi");
            }
            catch (Exception ex)
            {
                return ApiResponse<BulkCreateQuestionsResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }
    }
}
