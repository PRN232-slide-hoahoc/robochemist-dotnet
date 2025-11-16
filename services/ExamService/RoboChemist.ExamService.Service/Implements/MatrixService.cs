using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RoboChemist.ExamService.Model.Models;
using RoboChemist.ExamService.Repository.Interfaces;
using RoboChemist.ExamService.Service.Interfaces;
using RoboChemist.ExamService.Service.HttpClients;
using RoboChemist.Shared.Common.Constants;
using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.ExamServiceDTOs.MatrixDTOs;
using static RoboChemist.Shared.DTOs.TopicDTOs.TopicDTOs;

namespace RoboChemist.ExamService.Service.Implements
{
    /// <summary>
    /// Service quản lý ma trận đề thi
    /// </summary>
    public class MatrixService : IMatrixService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISlidesServiceHttpClient _slidesClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthServiceClient _authServiceClient;
        private readonly ILogger<MatrixService> _logger;

        public MatrixService(
            IUnitOfWork unitOfWork, 
            ISlidesServiceHttpClient slidesClient, 
            IHttpContextAccessor httpContextAccessor,
            IAuthServiceClient authServiceClient,
            ILogger<MatrixService> logger)
        {
            _unitOfWork = unitOfWork;
            _slidesClient = slidesClient;
            _httpContextAccessor = httpContextAccessor;
            _authServiceClient = authServiceClient;
            _logger = logger;
            _authServiceClient = authServiceClient;
        }

        /// <summary>
        /// Lấy thông tin ma trận theo ID (chỉ đọc)
        /// </summary>
        public async Task<ApiResponse<MatrixResponseDto>> GetMatrixByIdAsync(Guid id)
        {
            try
            {
                // Lấy Matrix từ database
                var matrix = await _unitOfWork.Matrices.GetByIdAsync(id);

                if (matrix == null)
                {
                    return ApiResponse<MatrixResponseDto>.ErrorResult($"Không tìm thấy ma trận với ID: {id}");
                }

                // Load MatrixDetails cho matrix này
                matrix.Matrixdetails = await _unitOfWork.MatrixDetails.GetByMatrixIdAsync(id);

                // Map sang ResponseDto và enrich với TopicName từ SlidesService
                var response = await MapToResponseDto(matrix);
                return ApiResponse<MatrixResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<MatrixResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả ma trận đề thi (chỉ đọc)
        /// Role-based: User chỉ thấy ma trận do mình tạo, Teacher/Admin thấy tất cả
        /// </summary>
        public async Task<ApiResponse<List<MatrixResponseDto>>> GetAllMatricesAsync(bool? isActive = null)
        {
            try
            {
                // Lấy thông tin user hiện tại
                var currentUser = await _authServiceClient.GetCurrentUserAsync();
                
                // Lấy matrices từ repository với query logic đã được tối ưu
                List<Matrix> allMatrices;
                if (currentUser != null && currentUser.Role == "User")
                {
                    // User chỉ thấy ma trận do mình tạo
                    allMatrices = await _unitOfWork.Matrices.GetMatricesByUserAsync(currentUser.Id, isActive);
                }
                else
                {
                    // Teacher và Admin thấy tất cả
                    allMatrices = await _unitOfWork.Matrices.GetMatricesByStatusAsync(isActive);
                }

                // Load MatrixDetails cho tất cả matrices
                foreach (var matrix in allMatrices)
                {
                    matrix.Matrixdetails = await _unitOfWork.MatrixDetails.GetByMatrixIdAsync(matrix.MatrixId);
                }

                // BATCH GET Topics - Thu thập TẤT CẢ TopicIds từ TẤT CẢ matrices
                var allTopicIds = allMatrices
                    .SelectMany(m => m.Matrixdetails)
                    .Where(d => d.TopicId.HasValue)
                    .Select(d => d.TopicId!.Value)
                    .Distinct()
                    .ToList();

                // Gọi 1 LẦN DUY NHẤT để lấy tất cả topics (tránh N+1 query)
                var topicsDict = await _slidesClient.GetTopicsByIdsAsync(allTopicIds);

                // Map tất cả matrices với topics đã cache
                var response = allMatrices.Select(matrix => MapToResponseDtoSync(matrix, topicsDict)).ToList();

                return ApiResponse<List<MatrixResponseDto>>.SuccessResult(response, $"Tìm thấy {response.Count} ma trận");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<MatrixResponseDto>>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper: Map Matrix entity sang MatrixResponseDto với topics đã cache (SYNC - không gọi API)
        /// </summary>
        private MatrixResponseDto MapToResponseDtoSync(Matrix matrix, Dictionary<Guid, TopicDto> topicsDict)
        {
            var response = new MatrixResponseDto
            {
                MatrixId = matrix.MatrixId,
                Name = matrix.Name,
                TotalQuestion = matrix.TotalQuestion ?? 0,
                IsActive = matrix.IsActive ?? true,
                CreatedBy = matrix.CreatedBy,
                CreatedAt = matrix.CreatedAt,
                MatrixDetails = new List<MatrixDetailResponseDto>()
            };

            foreach (var detail in matrix.Matrixdetails)
            {
                var topicId = detail.TopicId ?? Guid.Empty;
                var topicName = topicsDict.TryGetValue(topicId, out var topic)
                    ? topic.Name
                    : $"Topic {topicId}";

                response.MatrixDetails.Add(new MatrixDetailResponseDto
                {
                    MatrixDetailsId = detail.MatrixDetailsId,
                    TopicId = topicId,
                    TopicName = topicName,
                    QuestionType = detail.QuestionType,
                    Level = detail.Level,
                    QuestionCount = detail.QuestionCount,
                    IsActive = detail.IsActive ?? true
                });
            }

            return response;
        }

        /// <summary>
        /// Helper: Map Matrix entity sang MatrixResponseDto và enrich với TopicName từ SlidesService
        /// </summary>
        private async Task<MatrixResponseDto> MapToResponseDto(Matrix matrix)
        {
            var response = new MatrixResponseDto
            {
                MatrixId = matrix.MatrixId,
                Name = matrix.Name,
                TotalQuestion = matrix.TotalQuestion ?? 0,
                IsActive = matrix.IsActive ?? true,
                CreatedBy = matrix.CreatedBy,
                CreatedAt = matrix.CreatedAt,
                MatrixDetails = new List<MatrixDetailResponseDto>()
            };

            // BATCH GET Topics - Thu thập tất cả TopicIds duy nhất
            var topicIds = matrix.Matrixdetails
                .Where(d => d.TopicId.HasValue)
                .Select(d => d.TopicId!.Value)
                .Distinct()
                .ToList();

            // Gọi 1 lần duy nhất để lấy tất cả topics (tránh N+1 query)
            var topicsDict = await _slidesClient.GetTopicsByIdsAsync(topicIds);

            // Map với topics đã cache
            foreach (var detail in matrix.Matrixdetails)
            {
                var topicId = detail.TopicId ?? Guid.Empty;
                var topicName = topicsDict.TryGetValue(topicId, out var topic)
                    ? topic.Name
                    : $"Topic {topicId}";

                response.MatrixDetails.Add(new MatrixDetailResponseDto
                {
                    MatrixDetailsId = detail.MatrixDetailsId,
                    TopicId = topicId,
                    TopicName = topicName,
                    QuestionType = detail.QuestionType,
                    Level = detail.Level,
                    QuestionCount = detail.QuestionCount,
                    IsActive = detail.IsActive ?? true
                });
            }

            return response;
        }

        public async Task<ApiResponse<MatrixResponseDto>> CreateMatrixAsync(CreateMatrixDto createDto)
        {
            try
            {
                var currentUser = await _authServiceClient.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    _logger.LogWarning("CreateMatrix failed: User not authenticated");
                    return ApiResponse<MatrixResponseDto>.ErrorResult("Người dùng chưa xác thực");
                }

                _logger.LogInformation("CreateMatrix started: name={Name}, totalQuestions={TotalQuestions}, userId={UserId}", 
                    createDto.Name, createDto.TotalQuestion, currentUser.Id);

                if (createDto == null) 
                {
                    return ApiResponse<MatrixResponseDto>.ErrorResult("Payload không hợp lệ");
                }

                var validQuestionTypes = new[] 
                { 
                    RoboChemistConstants.QUESTION_TYPE_MULTIPLE_CHOICE,
                    RoboChemistConstants.QUESTION_TYPE_TRUE_FALSE,
                    RoboChemistConstants.QUESTION_TYPE_FILL_BLANK,
                    RoboChemistConstants.QUESTION_TYPE_ESSAY
                };

                foreach (var detail in createDto.MatrixDetails)
                {
                    if (!validQuestionTypes.Contains(detail.QuestionType))
                    {
                        _logger.LogWarning("Invalid question type: {QuestionType}", detail.QuestionType);
                        return ApiResponse<MatrixResponseDto>.ErrorResult(
                            $"Loại câu hỏi '{detail.QuestionType}' không hợp lệ. Chỉ chấp nhận: MultipleChoice, TrueFalse, FillBlank, Essay");
                    }
                }

                var sum = createDto.MatrixDetails.Sum(d => d.QuestionCount);
                if (sum != createDto.TotalQuestion)
                {
                    _logger.LogWarning("Question count mismatch: sum={Sum}, totalQuestion={TotalQuestion}", sum, createDto.TotalQuestion);
                    return ApiResponse<MatrixResponseDto>.ErrorResult($"Tổng số câu của các chi tiết ({sum}) không khớp với TotalQuestion ({createDto.TotalQuestion})");
                }

                foreach (var detail in createDto.MatrixDetails)
                {
                    var topicResponse = await _slidesClient.GetTopicByIdAsync(detail.TopicId);
                    if (topicResponse?.Success != true || topicResponse.Data == null)
                    {
                        _logger.LogWarning("Topic not found: topicId={TopicId}", detail.TopicId);
                        return ApiResponse<MatrixResponseDto>.ErrorResult($"Không tìm thấy Topic với ID: {detail.TopicId}");
                    }
                }

                foreach (var detail in createDto.MatrixDetails)
                {
                    var availableQuestionCount = await _unitOfWork.Questions.CountQuestionsByFiltersAsync(
                        detail.TopicId,
                        detail.QuestionType,
                        detail.Level,
                        isActive: true
                    );

                    if (availableQuestionCount < detail.QuestionCount)
                    {
                        var levelInfo = string.IsNullOrEmpty(detail.Level) ? "" : $", Level '{detail.Level}'";
                        var errorMsg = $"Không đủ câu hỏi cho Topic {detail.TopicId}, QuestionType '{detail.QuestionType}'{levelInfo}. " +
                            $"Yêu cầu {detail.QuestionCount} câu, chỉ có {availableQuestionCount} câu trong hệ thống";
                        
                        _logger.LogError("Matrix validation failed: {ErrorMessage}", errorMsg);
                        return ApiResponse<MatrixResponseDto>.ErrorResult(errorMsg);
                    }
                }

                var matrix = new Matrix
                {
                    MatrixId = Guid.NewGuid(),
                    Name = createDto.Name,
                    TotalQuestion = createDto.TotalQuestion,
                    CreatedAt = DateTime.Now,
                    CreatedBy = currentUser.Id,
                    IsActive = true
                };

                await _unitOfWork.Matrices.CreateAsync(matrix);

                foreach (var detail in createDto.MatrixDetails)
                {
                    var md = new Matrixdetail
                    {
                        MatrixDetailsId = Guid.NewGuid(),
                        MatrixId = matrix.MatrixId,
                        TopicId = detail.TopicId,
                        QuestionType = detail.QuestionType,
                        Level = detail.Level,
                        QuestionCount = detail.QuestionCount,
                        CreatedAt = DateTime.Now,
                        CreatedBy = currentUser.Id,
                        IsActive = true
                    };

                    await _unitOfWork.MatrixDetails.CreateAsync(md);
                }

                var created = await _unitOfWork.Matrices.GetByIdAsync(matrix.MatrixId);
                if (created == null) 
                {
                    return ApiResponse<MatrixResponseDto>.ErrorResult("Tạo ma trận thất bại");
                }

                created.Matrixdetails = await _unitOfWork.MatrixDetails.GetByMatrixIdAsync(created.MatrixId);

                var response = await MapToResponseDto(created);
                _logger.LogInformation("CreateMatrix completed successfully: matrixId={MatrixId}", matrix.MatrixId);
                return ApiResponse<MatrixResponseDto>.SuccessResult(response, "Tạo ma trận thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateMatrix failed");
                return ApiResponse<MatrixResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }
    }
}
