using RoboChemist.ExamService.Model.Models;
using RoboChemist.ExamService.Repository.Interfaces;
using RoboChemist.ExamService.Service.Interfaces;
using RoboChemist.ExamService.Service.HttpClients;
using RoboChemist.Shared.Common.Constants;
using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.ExamServiceDTOs.ExamDTOs;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.WalletTransactionDTOs;
using Microsoft.AspNetCore.Http;

namespace RoboChemist.ExamService.Service.Implements
{
    /// <summary>
    /// Implementation của Exam Service - Quản lý việc tạo và quản lý đề thi
    /// </summary>
    public class ExamService : IExamService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWalletServiceHttpClient _walletServiceHttpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExamService(IUnitOfWork unitOfWork, IHttpClientFactory httpClientFactory, IWalletServiceHttpClient walletServiceHttpClient, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
            _walletServiceHttpClient = walletServiceHttpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Tạo yêu cầu tạo đề thi mới với SAGA pattern (Payment -> Create -> Process -> Compensate if failed)
        /// </summary>
        public async Task<ApiResponse<ExamRequestResponseDto>> CreateExamRequestAsync(CreateExamRequestDto createExamRequestDto, Guid userId)
        {
            Guid? examRequestId = null;
            PaymentResponseDto? paymentResponse = null;

            try
            {
                // Extract JWT token from HttpContext
                var authToken = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");
                if (string.IsNullOrEmpty(authToken))
                {
                    return ApiResponse<ExamRequestResponseDto>.ErrorResult("Không tìm thấy token xác thực");
                }

                // SAGA Step 1: Validate Matrix
                var matrix = await _unitOfWork.Matrices.GetByIdAsync(createExamRequestDto.MatrixId);
                if (matrix == null)
                {
                    return ApiResponse<ExamRequestResponseDto>.ErrorResult($"Không tìm thấy ma trận với ID: {createExamRequestDto.MatrixId}");
                }

                if (matrix.IsActive == false)
                {
                    return ApiResponse<ExamRequestResponseDto>.ErrorResult("Ma trận đã bị vô hiệu hóa");
                }

                // SAGA Step 2: Create Payment Transaction (Deduct money from wallet)
                Console.WriteLine($"[SAGA] Step 2: Creating payment for user {userId}, amount {createExamRequestDto.Price}");
                
                var createPaymentDto = new CreatePaymentDto
                {
                    UserId = userId,
                    Amount = createExamRequestDto.Price,
                    ReferenceId = Guid.NewGuid(), // Tạo temporary ID cho ExamRequest
                    ReferenceType = "ExamRequest",
                    Description = $"Thanh toán tạo đề thi từ ma trận {matrix.Name}"
                };

                examRequestId = createPaymentDto.ReferenceId; // Lưu lại để dùng cho ExamRequest

                paymentResponse = await _walletServiceHttpClient.CreatePaymentAsync(createPaymentDto, authToken);
                
                if (paymentResponse == null)
                {
                    Console.WriteLine("[SAGA] Step 2 FAILED: Payment creation failed");
                    return ApiResponse<ExamRequestResponseDto>.ErrorResult("Thanh toán thất bại. Vui lòng kiểm tra số dư ví");
                }

                Console.WriteLine($"[SAGA] Step 2 SUCCESS: Payment created, TransactionId={paymentResponse.TransactionId}");

                // Quăng lỗi để test SAGA Compensation
                //throw new Exception("TESTING COMPENSATION");

                // SAGA Step 3: Create ExamRequest entity
                Console.WriteLine($"[SAGA] Step 3: Creating ExamRequest with ID={examRequestId}");
                
                var examRequest = new Examrequest
                {
                    ExamRequestId = examRequestId.Value,
                    UserId = userId,
                    MatrixId = createExamRequestDto.MatrixId,
                    Status = RoboChemistConstants.EXAMREQ_STATUS_PENDING,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.ExamRequests.CreateAsync(examRequest);

                Console.WriteLine($"[SAGA] Step 3 SUCCESS: ExamRequest created");

                // SAGA Step 4: Map response
                var response = new ExamRequestResponseDto
                {
                    ExamRequestId = examRequest.ExamRequestId,
                    UserId = examRequest.UserId,
                    MatrixId = examRequest.MatrixId,
                    MatrixName = matrix.Name,
                    Status = examRequest.Status,
                    CreatedAt = examRequest.CreatedAt,
                    GeneratedExams = new List<GeneratedExamResponseDto>()
                };

                Console.WriteLine("[SAGA] COMPLETED: ExamRequest created successfully with payment");
                return ApiResponse<ExamRequestResponseDto>.SuccessResult(response, "Tạo yêu cầu tạo đề thi thành công");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SAGA ERROR] Exception occurred: {ex.Message}");

                // SAGA Compensation: Refund payment if payment was successful
                // Logic: Nếu đã trừ tiền (paymentResponse != null) thì phải hoàn tiền
                if (paymentResponse != null)
                {
                    Console.WriteLine($"[SAGA COMPENSATE] Payment was successful but process failed. Initiating refund for ReferenceId={examRequestId}");
                    
                    try
                    {
                        var authToken = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");
                        
                        var refundRequest = new RefundRequestDto
                        {
                            ReferenceId = examRequestId!.Value,
                            Reason = $"Tạo đề thi thất bại: {ex.Message}"
                        };

                        var refundResponse = await _walletServiceHttpClient.RefundPaymentAsync(refundRequest, authToken);
                        
                        if (refundResponse != null)
                        {
                            Console.WriteLine($"[SAGA COMPENSATE] SUCCESS: Refund completed, RefundTransactionId={refundResponse.RefundTransactionId}");
                            return ApiResponse<ExamRequestResponseDto>.ErrorResult($"Tạo đề thi thất bại và đã hoàn tiền: {ex.Message}");
                        }
                        else
                        {
                            Console.WriteLine("[SAGA COMPENSATE] WARNING: Refund failed - manual intervention required");
                            return ApiResponse<ExamRequestResponseDto>.ErrorResult($"Tạo đề thi thất bại và hoàn tiền thất bại. Vui lòng liên hệ hỗ trợ. Error: {ex.Message}");
                        }
                    }
                    catch (Exception refundEx)
                    {
                        Console.WriteLine($"[SAGA COMPENSATE ERROR] Refund exception: {refundEx.Message}");
                        return ApiResponse<ExamRequestResponseDto>.ErrorResult($"Tạo đề thi thất bại và hoàn tiền gặp lỗi. Vui lòng liên hệ hỗ trợ. Errors: {ex.Message} | Refund: {refundEx.Message}");
                    }
                }

                // Nếu payment chưa thực hiện (paymentResponse == null) thì chỉ trả lỗi thông thường
                return ApiResponse<ExamRequestResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin yêu cầu tạo đề theo ID
        /// </summary>
        public async Task<ApiResponse<ExamRequestResponseDto>> GetExamRequestByIdAsync(Guid examRequestId)
        {
            try
            {
                var examRequest = await _unitOfWork.ExamRequests.GetByIdAsync(examRequestId);
                if (examRequest == null)
                {
                    return ApiResponse<ExamRequestResponseDto>.ErrorResult($"Không tìm thấy yêu cầu tạo đề với ID: {examRequestId}");
                }

                // Lấy Matrix info
                var matrix = await _unitOfWork.Matrices.GetByIdAsync(examRequest.MatrixId);

                // Lấy các GeneratedExam liên quan
                var allGeneratedExams = await _unitOfWork.GeneratedExams.GetAllAsync();
                var generatedExams = allGeneratedExams.Where(ge => ge.ExamRequestId == examRequestId).ToList();

                var response = new ExamRequestResponseDto
                {
                    ExamRequestId = examRequest.ExamRequestId,
                    UserId = examRequest.UserId,
                    MatrixId = examRequest.MatrixId,
                    MatrixName = matrix?.Name ?? "Unknown Matrix",
                    // GradeId, GradeName, Prompt đã BỎ
                    Status = examRequest.Status,
                    CreatedAt = examRequest.CreatedAt,
                    GeneratedExams = generatedExams.Select(ge => new GeneratedExamResponseDto
                    {
                        GeneratedExamId = ge.GeneratedExamId,
                        ExamRequestId = ge.ExamRequestId,
                        Status = ge.Status,
                        CreatedAt = ge.CreatedAt,
                        ExamQuestions = new List<ExamQuestionResponseDto>()
                    }).ToList()
                };

                return ApiResponse<ExamRequestResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<ExamRequestResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách yêu cầu tạo đề của người dùng
        /// </summary>
        public async Task<ApiResponse<List<ExamRequestResponseDto>>> GetExamRequestsByUserAsync(Guid userId, string? status = null)
        {
            try
            {
                var allExamRequests = await _unitOfWork.ExamRequests.GetAllAsync();
                
                // Filter theo userId
                var userExamRequests = allExamRequests.Where(er => er.UserId == userId);

                // Filter theo status nếu có
                if (!string.IsNullOrEmpty(status))
                {
                    userExamRequests = userExamRequests.Where(er => er.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
                }

                var examRequests = userExamRequests.OrderByDescending(er => er.CreatedAt).ToList();

                var response = new List<ExamRequestResponseDto>();
                foreach (var examRequest in examRequests)
                {
                    var matrix = await _unitOfWork.Matrices.GetByIdAsync(examRequest.MatrixId);
                    
                    response.Add(new ExamRequestResponseDto
                    {
                        ExamRequestId = examRequest.ExamRequestId,
                        UserId = examRequest.UserId,
                        MatrixId = examRequest.MatrixId,
                        MatrixName = matrix?.Name ?? "Unknown Matrix",
                        // GradeId, GradeName, Prompt đã BỎ
                        Status = examRequest.Status,
                        CreatedAt = examRequest.CreatedAt,
                        GeneratedExams = new List<GeneratedExamResponseDto>()
                    });
                }

                return ApiResponse<List<ExamRequestResponseDto>>.SuccessResult(response, $"Tìm thấy {response.Count} yêu cầu tạo đề");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ExamRequestResponseDto>>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Xử lý tạo đề thi từ yêu cầu (Generate exam từ matrix và AI)
        /// </summary>
        public async Task<ApiResponse<GeneratedExamResponseDto>> GenerateExamAsync(Guid examRequestId)
        {
            try
            {
                // 1. Lấy ExamRequest
                var examRequest = await _unitOfWork.ExamRequests.GetByIdAsync(examRequestId);
                if (examRequest == null)
                {
                    return ApiResponse<GeneratedExamResponseDto>.ErrorResult($"Không tìm thấy yêu cầu tạo đề với ID: {examRequestId}");
                }

                // 2. Kiểm tra trạng thái
                if (examRequest.Status != "Pending")
                {
                    return ApiResponse<GeneratedExamResponseDto>.ErrorResult($"Yêu cầu đã được xử lý (Status: {examRequest.Status})");
                }

                // 3. Lấy Matrix và MatrixDetails
                var matrix = await _unitOfWork.Matrices.GetByIdAsync(examRequest.MatrixId);
                if (matrix == null)
                {
                    return ApiResponse<GeneratedExamResponseDto>.ErrorResult("Không tìm thấy ma trận đề thi");
                }

                var allMatrixDetails = await _unitOfWork.MatrixDetails.GetAllAsync();
                var matrixDetails = allMatrixDetails.Where(md => md.MatrixId == matrix.MatrixId && md.IsActive == true).ToList();

                if (!matrixDetails.Any())
                {
                    return ApiResponse<GeneratedExamResponseDto>.ErrorResult("Ma trận không có chi tiết phân bổ câu hỏi");
                }

                // 4. Tạo GeneratedExam
                var generatedExam = new Generatedexam
                {
                    GeneratedExamId = Guid.NewGuid(),
                    ExamRequestId = examRequestId,
                    Status = "Draft", // Trạng thái khởi tạo của đề thi
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.GeneratedExams.CreateAsync(generatedExam);

                // 5. Lấy câu hỏi theo từng MatrixDetail và tạo ExamQuestion
                var allQuestions = await _unitOfWork.Questions.GetAllAsync();
                var examQuestions = new List<Examquestion>();
                int questionOrder = 1;

                foreach (var detail in matrixDetails)
                {
                    // Lọc câu hỏi theo TopicId, QuestionType và IsActive
                    var matchingQuestions = allQuestions
                        .Where(q => q.TopicId == detail.TopicId 
                                 && q.QuestionType == detail.QuestionType 
                                 && q.IsActive == true)
                        .OrderBy(x => Guid.NewGuid()) // Random order
                        .Take(detail.QuestionCount)
                        .ToList();

                    if (matchingQuestions.Count < detail.QuestionCount)
                    {
                        return ApiResponse<GeneratedExamResponseDto>.ErrorResult(
                            $"Không đủ câu hỏi cho Topic {detail.TopicId}, Type {detail.QuestionType}. Cần {detail.QuestionCount}, chỉ có {matchingQuestions.Count}");
                    }

                    // Tạo ExamQuestion cho mỗi câu hỏi được chọn
                    foreach (var question in matchingQuestions)
                    {
                        var examQuestion = new Examquestion
                        {
                            ExamQuestionId = Guid.NewGuid(),
                            GeneratedExamId = generatedExam.GeneratedExamId,
                            QuestionId = question.QuestionId,
                            Status = "Active",
                            CreatedAt = DateTime.Now
                        };

                        await _unitOfWork.ExamQuestions.CreateAsync(examQuestion);
                        examQuestions.Add(examQuestion);
                        questionOrder++;
                    }
                }

                // 6. Cập nhật trạng thái ExamRequest
                examRequest.Status = "Completed";
                await _unitOfWork.ExamRequests.UpdateAsync(examRequest);
                await _unitOfWork.SaveChangesAsync();

                // 7. Map sang ResponseDto
                var response = new GeneratedExamResponseDto
                {
                    GeneratedExamId = generatedExam.GeneratedExamId,
                    ExamRequestId = generatedExam.ExamRequestId,
                    Status = generatedExam.Status,
                    CreatedAt = generatedExam.CreatedAt,
                    ExamQuestions = examQuestions.Select((eq, index) => new ExamQuestionResponseDto
                    {
                        ExamQuestionId = eq.ExamQuestionId,
                        GeneratedExamId = eq.GeneratedExamId,
                        QuestionId = eq.QuestionId,
                        QuestionOrder = index + 1,
                        Points = 1.0m
                    }).ToList()
                };

                return ApiResponse<GeneratedExamResponseDto>.SuccessResult(response, "Tạo đề thi thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<GeneratedExamResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin đề thi đã được tạo theo ID
        /// </summary>
        public async Task<ApiResponse<GeneratedExamResponseDto>> GetGeneratedExamByIdAsync(Guid generatedExamId)
        {
            try
            {
                var generatedExam = await _unitOfWork.GeneratedExams.GetByIdAsync(generatedExamId);
                if (generatedExam == null)
                {
                    return ApiResponse<GeneratedExamResponseDto>.ErrorResult($"Không tìm thấy đề thi với ID: {generatedExamId}");
                }

                // Lấy ExamQuestions
                var allExamQuestions = await _unitOfWork.ExamQuestions.GetAllAsync();
                var examQuestions = allExamQuestions.Where(eq => eq.GeneratedExamId == generatedExamId).ToList();

                // Lấy Questions và Options
                var allQuestions = await _unitOfWork.Questions.GetAllAsync();
                var allOptions = await _unitOfWork.Options.GetAllAsync();

                var response = new GeneratedExamResponseDto
                {
                    GeneratedExamId = generatedExam.GeneratedExamId,
                    ExamRequestId = generatedExam.ExamRequestId,
                    Status = generatedExam.Status,
                    CreatedAt = generatedExam.CreatedAt,
                    ExamQuestions = examQuestions.Select((eq, index) =>
                    {
                        var question = allQuestions.FirstOrDefault(q => q.QuestionId == eq.QuestionId);
                        var options = allOptions.Where(o => o.QuestionId == eq.QuestionId).ToList();

                        return new ExamQuestionResponseDto
                        {
                            ExamQuestionId = eq.ExamQuestionId,
                            GeneratedExamId = eq.GeneratedExamId,
                            QuestionId = eq.QuestionId,
                            QuestionOrder = index + 1,
                            Points = 1.0m,
                            QuestionDetail = question == null ? null : new QuestionDetailDto
                            {
                                QuestionId = question.QuestionId,
                                QuestionType = question.QuestionType,
                                QuestionText = question.QuestionText,  // Đã đổi từ Question1
                                Explanation = question.Explanation,
                                Options = options.Select(o => new OptionDetailDto
                                {
                                    OptionId = o.OptionId,
                                    Answer = o.Answer,
                                    IsCorrect = o.IsCorrect ?? false
                                }).ToList()
                            }
                        };
                    }).ToList()
                };

                return ApiResponse<GeneratedExamResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<GeneratedExamResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật trạng thái đề thi (Draft -> Published -> Archived)
        /// </summary>
        public async Task<ApiResponse<GeneratedExamResponseDto>> UpdateExamStatusAsync(Guid generatedExamId, string status)
        {
            try
            {
                var generatedExam = await _unitOfWork.GeneratedExams.GetByIdAsync(generatedExamId);
                if (generatedExam == null)
                {
                    return ApiResponse<GeneratedExamResponseDto>.ErrorResult($"Không tìm thấy đề thi với ID: {generatedExamId}");
                }

                // Validate status hợp lệ
                var validStatuses = new[] { "Draft", "Published", "Archived" };
                if (!validStatuses.Contains(status))
                {
                    return ApiResponse<GeneratedExamResponseDto>.ErrorResult($"Trạng thái không hợp lệ. Chỉ chấp nhận: {string.Join(", ", validStatuses)}");
                }

                generatedExam.Status = status;
                await _unitOfWork.GeneratedExams.UpdateAsync(generatedExam);
                await _unitOfWork.SaveChangesAsync();

                // Get full details
                var result = await GetGeneratedExamByIdAsync(generatedExamId);
                if (!result.Success || result.Data == null)
                {
                    return ApiResponse<GeneratedExamResponseDto>.ErrorResult("Cập nhật thành công nhưng không thể lấy thông tin đề thi");
                }
                
                return ApiResponse<GeneratedExamResponseDto>.SuccessResult(result.Data, "Cập nhật trạng thái đề thi thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<GeneratedExamResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa đề thi đã được tạo
        /// </summary>
        public async Task<ApiResponse<bool>> DeleteGeneratedExamAsync(Guid generatedExamId)
        {
            try
            {
                var generatedExam = await _unitOfWork.GeneratedExams.GetByIdAsync(generatedExamId);
                if (generatedExam == null)
                {
                    return ApiResponse<bool>.ErrorResult($"Không tìm thấy đề thi với ID: {generatedExamId}");
                }

                // Xóa các ExamQuestion liên quan
                var allExamQuestions = await _unitOfWork.ExamQuestions.GetAllAsync();
                var examQuestions = allExamQuestions.Where(eq => eq.GeneratedExamId == generatedExamId).ToList();

                foreach (var examQuestion in examQuestions)
                {
                    await _unitOfWork.ExamQuestions.RemoveAsync(examQuestion);
                }

                // Xóa GeneratedExam
                await _unitOfWork.GeneratedExams.RemoveAsync(generatedExam);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Xóa đề thi thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }
    }
}
