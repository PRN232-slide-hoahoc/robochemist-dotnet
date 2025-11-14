using System.Text.Json;
using Microsoft.AspNetCore.Http;
using RoboChemist.ExamService.Model.Models;
using RoboChemist.ExamService.Repository.Interfaces;
using RoboChemist.ExamService.Service.Interfaces;
using RoboChemist.ExamService.Service.HttpClients;
using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.ExamServiceDTOs.MatrixDTOs;

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

        public MatrixService(IUnitOfWork unitOfWork, ISlidesServiceHttpClient slidesClient, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _slidesClient = slidesClient;
            _httpContextAccessor = httpContextAccessor;
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
        /// </summary>
        public async Task<ApiResponse<List<MatrixResponseDto>>> GetAllMatricesAsync(bool? isActive = null)
        {
            try
            {
                // Lấy tất cả matrices
                var allMatrices = await _unitOfWork.Matrices.GetAllAsync();

                // Lọc theo isActive nếu có
                if (isActive.HasValue)
                {
                    allMatrices = allMatrices.Where(m => m.IsActive == isActive.Value).ToList();
                }

                // Sắp xếp theo CreatedAt giảm dần (mới nhất lên đầu)
                allMatrices = allMatrices.OrderByDescending(m => m.CreatedAt).ToList();

                var response = new List<MatrixResponseDto>();
                foreach (var matrix in allMatrices)
                {
                    // Load MatrixDetails cho từng matrix
                    matrix.Matrixdetails = await _unitOfWork.MatrixDetails.GetByMatrixIdAsync(matrix.MatrixId);
                    
                    // Map sang ResponseDto và enrich với TopicName
                    response.Add(await MapToResponseDto(matrix));
                }

                return ApiResponse<List<MatrixResponseDto>>.SuccessResult(response, $"Tìm thấy {response.Count} ma trận");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<MatrixResponseDto>>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
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

            // Enrich với TopicName từ SlidesService thông qua SlidesServiceHttpClient
            foreach (var detail in matrix.Matrixdetails)
            {
                string topicName = "Unknown Topic";
                try
                {
                    var topicResponse = await _slidesClient.GetTopicByIdAsync(detail.TopicId ?? Guid.Empty);
                    if (topicResponse?.Success == true && topicResponse.Data != null)
                    {
                        topicName = topicResponse.Data.Name;
                    }
                }
                catch
                {
                    topicName = $"Topic {detail.TopicId ?? Guid.Empty}";
                }

                response.MatrixDetails.Add(new MatrixDetailResponseDto
                {
                    MatrixDetailsId = detail.MatrixDetailsId,
                    TopicId = detail.TopicId ?? Guid.Empty,
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
        /// Tạo mới ma trận đề thi kèm chi tiết
        /// </summary>
        public async Task<ApiResponse<MatrixResponseDto>> CreateMatrixAsync(CreateMatrixDto createDto, Guid? createdBy = null)
        {
            try
            {
                if (createDto == null) return ApiResponse<MatrixResponseDto>.ErrorResult("Payload không hợp lệ");

                // Validate QuestionType của từng detail trước
                var validQuestionTypes = new[] 
                { 
                    RoboChemist.Shared.Common.Constants.RoboChemistConstants.QUESTION_TYPE_MULTIPLE_CHOICE,
                    RoboChemist.Shared.Common.Constants.RoboChemistConstants.QUESTION_TYPE_TRUE_FALSE,
                    RoboChemist.Shared.Common.Constants.RoboChemistConstants.QUESTION_TYPE_FILL_BLANK,
                    RoboChemist.Shared.Common.Constants.RoboChemistConstants.QUESTION_TYPE_ESSAY
                };

                foreach (var detail in createDto.MatrixDetails)
                {
                    if (!validQuestionTypes.Contains(detail.QuestionType))
                    {
                        return ApiResponse<MatrixResponseDto>.ErrorResult(
                            $"Loại câu hỏi '{detail.QuestionType}' không hợp lệ. Chỉ chấp nhận: MultipleChoice, TrueFalse, FillBlank, Essay");
                    }
                }

                // Kiểm tra tổng câu chi tiết có khớp với TotalQuestion hay không
                var sum = createDto.MatrixDetails.Sum(d => d.QuestionCount);
                if (sum != createDto.TotalQuestion)
                {
                    return ApiResponse<MatrixResponseDto>.ErrorResult($"Tổng số câu của các chi tiết ({sum}) không khớp với TotalQuestion ({createDto.TotalQuestion})");
                }

                // Kiểm tra từng Topic tồn tại trong SlidesService
                foreach (var detail in createDto.MatrixDetails)
                {
                    var topicResponse = await _slidesClient.GetTopicByIdAsync(detail.TopicId);
                    if (topicResponse?.Success != true || topicResponse.Data == null)
                    {
                        return ApiResponse<MatrixResponseDto>.ErrorResult($"Không tìm thấy Topic với ID: {detail.TopicId}");
                    }
                }

                // Tạo Matrix entity
                var matrix = new Matrix
                {
                    MatrixId = Guid.NewGuid(),
                    Name = createDto.Name,
                    TotalQuestion = createDto.TotalQuestion,
                    CreatedAt = DateTime.Now,
                    CreatedBy = createdBy,
                    IsActive = true
                };

                // Thêm vào DB
                await _unitOfWork.Matrices.CreateAsync(matrix);

                // Tạo MatrixDetail entities
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
                        CreatedBy = createdBy,
                        IsActive = true
                    };

                    await _unitOfWork.MatrixDetails.CreateAsync(md);
                }

                // Reload matrix with details to return
                var created = await _unitOfWork.Matrices.GetByIdAsync(matrix.MatrixId);
                if (created == null) return ApiResponse<MatrixResponseDto>.ErrorResult("Tạo ma trận thất bại");

                created.Matrixdetails = await _unitOfWork.MatrixDetails.GetByMatrixIdAsync(created.MatrixId);

                var response = await MapToResponseDto(created);
                return ApiResponse<MatrixResponseDto>.SuccessResult(response, "Tạo ma trận thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<MatrixResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }
    }
}
