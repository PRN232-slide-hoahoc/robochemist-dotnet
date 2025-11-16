using RoboChemist.ExamService.Model.Models;
using RoboChemist.ExamService.Repository.Interfaces;
using RoboChemist.ExamService.Service.Interfaces;
using RoboChemist.ExamService.Service.HttpClients;
using RoboChemist.Shared.Common.Constants;
using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.ExamServiceDTOs.ExamDTOs;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.WalletTransactionDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace RoboChemist.ExamService.Service.Implements
{
    /// <summary>
    /// Implementation của Exam Service - Quản lý việc tạo và quản lý đề thi
    /// </summary>
    public class ExamService : IExamService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWalletServiceHttpClient _walletServiceHttpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWordExportService _wordExportService;
        private readonly ITemplateServiceClient _templateServiceClient;
        private readonly IAuthServiceClient _authServiceClient;
        private readonly ILogger<ExamService> _logger;

        public ExamService(
            IUnitOfWork unitOfWork, 
            IWalletServiceHttpClient walletServiceHttpClient, 
            IHttpContextAccessor httpContextAccessor,
            IWordExportService wordExportService,
            ITemplateServiceClient templateServiceClient,
            IAuthServiceClient authServiceClient,
            ILogger<ExamService> logger)
        {
            _unitOfWork = unitOfWork;
            _walletServiceHttpClient = walletServiceHttpClient;
            _httpContextAccessor = httpContextAccessor;
            _wordExportService = wordExportService;
            _templateServiceClient = templateServiceClient;
            _authServiceClient = authServiceClient;
            _logger = logger;
        }

        public async Task<ApiResponse<ExamRequestResponseDto>> CreateExamRequestAsync(CreateExamRequestDto createExamRequestDto)
        {
            Guid? examRequestId = null;
            PaymentResponseDto? paymentResponse = null;

            try
            {
                var currentUser = await _authServiceClient.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    _logger.LogWarning("CreateExamRequest failed: User not authenticated");
                    return ApiResponse<ExamRequestResponseDto>.ErrorResult("Người dùng chưa xác thực");
                }

                var userId = currentUser.Id;
                _logger.LogInformation("CreateExamRequest started for userId={UserId}, matrixId={MatrixId}, price={Price}", 
                    userId, createExamRequestDto.MatrixId, createExamRequestDto.Price);

                var authToken = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");
                if (string.IsNullOrEmpty(authToken))
                {
                    _logger.LogWarning("CreateExamRequest failed: Missing authentication token");
                    return ApiResponse<ExamRequestResponseDto>.ErrorResult("Không tìm thấy token xác thực");
                }

                var matrix = await _unitOfWork.Matrices.GetByIdAsync(createExamRequestDto.MatrixId);
                if (matrix == null)
                {
                    _logger.LogWarning("Matrix not found: matrixId={MatrixId}", createExamRequestDto.MatrixId);
                    return ApiResponse<ExamRequestResponseDto>.ErrorResult($"Không tìm thấy ma trận với ID: {createExamRequestDto.MatrixId}");
                }

                if (matrix.CreatedBy.HasValue && matrix.CreatedBy.Value != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to use matrix {MatrixId} created by {CreatedBy}", 
                        userId, createExamRequestDto.MatrixId, matrix.CreatedBy.Value);
                    return ApiResponse<ExamRequestResponseDto>.ErrorResult("Bạn chỉ có thể tạo đề thi từ ma trận do bạn tạo");
                }

                if (matrix.IsActive == false)
                {
                    _logger.LogWarning("Matrix is inactive: matrixId={MatrixId}", createExamRequestDto.MatrixId);
                    return ApiResponse<ExamRequestResponseDto>.ErrorResult("Ma trận đã bị vô hiệu hóa");
                }

                // Get MatrixDetails - Dùng repository method
                var matrixDetails = await _unitOfWork.MatrixDetails.GetActiveByMatrixIdAsync(matrix.MatrixId);

                if (!matrixDetails.Any())
                {
                    return ApiResponse<ExamRequestResponseDto>.ErrorResult("Ma trận không có chi tiết phân bổ câu hỏi");
                }

                // SAGA Step 2: Create Payment Transaction (Deduct money from wallet)
                Console.WriteLine($"[SAGA] Step 2: Creating payment for user {userId}, amount {createExamRequestDto.Price}");
                
                var createPaymentDto = new CreatePaymentDto
                {
                    UserId = userId,
                    Amount = createExamRequestDto.Price,
                    ReferenceId = Guid.NewGuid(),
                    ReferenceType = "ExamRequest",
                    Description = $"Thanh toán tạo đề thi từ ma trận {matrix.Name}"
                };

                examRequestId = createPaymentDto.ReferenceId;

                Console.WriteLine($"[SAGA DEBUG] Payment DTO: UserId={createPaymentDto.UserId}, Amount={createPaymentDto.Amount}, ReferenceId={createPaymentDto.ReferenceId}, ReferenceType={createPaymentDto.ReferenceType}");
                Console.WriteLine($"[SAGA DEBUG] Auth Token: {(string.IsNullOrEmpty(authToken) ? "NULL/EMPTY" : "EXISTS (" + authToken.Length + " chars)")}");

                paymentResponse = await _walletServiceHttpClient.CreatePaymentAsync(createPaymentDto, authToken);
                
                if (paymentResponse == null)
                {
                    Console.WriteLine("[SAGA] Step 2 FAILED: Payment creation failed");
                    return ApiResponse<ExamRequestResponseDto>.ErrorResult("Thanh toán thất bại. Vui lòng kiểm tra số dư ví");
                }

                Console.WriteLine($"[SAGA] Step 2 SUCCESS: Payment created, TransactionId={paymentResponse.TransactionId}");

                // SAGA Step 3: Create ExamRequest entity
                Console.WriteLine($"[SAGA] Step 3: Creating ExamRequest with ID={examRequestId}");
                
                var examRequest = new Examrequest
                {
                    ExamRequestId = examRequestId.Value,
                    UserId = userId,
                    MatrixId = createExamRequestDto.MatrixId,
                    Status = RoboChemistConstants.EXAMREQ_STATUS_PROCESSING,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.ExamRequests.CreateAsync(examRequest);
                Console.WriteLine($"[SAGA] Step 3 SUCCESS: ExamRequest created");

                // SAGA Step 4: Create GeneratedExam with PENDING status
                Console.WriteLine($"[SAGA] Step 4: Creating GeneratedExam");
                
                var generatedExam = new Generatedexam
                {
                    GeneratedExamId = Guid.NewGuid(),
                    ExamRequestId = examRequestId.Value,
                    Status = RoboChemistConstants.GENERATED_EXAM_STATUS_PENDING,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.GeneratedExams.CreateAsync(generatedExam);
                Console.WriteLine($"[SAGA] Step 4 SUCCESS: GeneratedExam created with ID={generatedExam.GeneratedExamId}");

                // SAGA Step 5: Random questions and create ExamQuestion
                Console.WriteLine($"[SAGA] Step 5: Generating random questions");

                var examQuestions = new List<Examquestion>();
                var selectedQuestionIds = new HashSet<Guid>(); // Track đã chọn để tránh trùng

                foreach (var detail in matrixDetails)
                {
                    // Lấy random questions từ repository - đã lọc TopicId, QuestionType, Level, IsActive
                    var matchingQuestions = await _unitOfWork.Questions.GetRandomQuestionsByFiltersAsync(
                        detail.TopicId!.Value, 
                        detail.QuestionType,
                        null, // Không filter theo level
                        detail.QuestionCount);

                    // Loại bỏ câu hỏi đã được chọn trước đó
                    var availableQuestions = matchingQuestions
                        .Where(q => !selectedQuestionIds.Contains(q.QuestionId))
                        .Take(detail.QuestionCount)
                        .ToList();

                    if (availableQuestions.Count < detail.QuestionCount)
                    {
                        var levelInfo = string.IsNullOrEmpty(detail.Level) ? "" : $", Level {detail.Level}";
                        throw new Exception(
                            $"Không đủ câu hỏi cho Topic {detail.TopicId}, Type {detail.QuestionType}{levelInfo}. " +
                            $"Cần {detail.QuestionCount}, chỉ có {availableQuestions.Count} (sau khi loại bỏ trùng)");
                    }

                    // Create ExamQuestion for each selected question
                    foreach (var question in availableQuestions)
                    {
                        var examQuestion = new Examquestion
                        {
                            ExamQuestionId = Guid.NewGuid(),
                            GeneratedExamId = generatedExam.GeneratedExamId,
                            QuestionId = question.QuestionId,
                            Status = RoboChemistConstants.EXAMQUESTION_STATUS_ACTIVE,
                            CreatedAt = DateTime.Now
                        };

                        await _unitOfWork.ExamQuestions.CreateAsync(examQuestion);
                        examQuestions.Add(examQuestion);
                        selectedQuestionIds.Add(question.QuestionId); // Mark đã chọn
                    }
                }

                Console.WriteLine($"[SAGA] Step 5 SUCCESS: Created {examQuestions.Count} exam questions");

                // SAGA Step 6: Update statuses
                generatedExam.Status = RoboChemistConstants.GENERATED_EXAM_STATUS_READY;
                await _unitOfWork.GeneratedExams.UpdateAsync(generatedExam);

                examRequest.Status = RoboChemistConstants.EXAMREQ_STATUS_COMPLETED;
                await _unitOfWork.ExamRequests.UpdateAsync(examRequest);
                
                await _unitOfWork.SaveChangesAsync();
                Console.WriteLine($"[SAGA] Step 6 SUCCESS: Updated statuses to READY and COMPLETED");

                // Map response
                var response = new ExamRequestResponseDto
                {
                    ExamRequestId = examRequest.ExamRequestId,
                    UserId = examRequest.UserId,
                    MatrixId = examRequest.MatrixId,
                    MatrixName = matrix.Name,
                    Status = examRequest.Status,
                    CreatedAt = examRequest.CreatedAt,
                    GeneratedExams = new List<GeneratedExamResponseDto>
                    {
                        new GeneratedExamResponseDto
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
                        }
                    }
                };

                Console.WriteLine("[SAGA] COMPLETED: ExamRequest created successfully with auto-generated questions");
                return ApiResponse<ExamRequestResponseDto>.SuccessResult(response, "Tạo đề thi và generate câu hỏi thành công");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SAGA ERROR] Exception occurred: {ex.Message}");

                // SAGA Compensation: Refund payment if payment was successful
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

                // Lấy các GeneratedExam liên quan - Dùng repository method
                var generatedExams = await _unitOfWork.GeneratedExams.GetByExamRequestIdAsync(examRequestId);

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
                // Lấy exam requests từ repository - đã filter và sort
                var examRequests = await _unitOfWork.ExamRequests.GetExamRequestsByUserAsync(userId, status);

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

                // Lấy MatrixDetails active từ repository
                var matrixDetails = await _unitOfWork.MatrixDetails.GetActiveByMatrixIdAsync(matrix.MatrixId);

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
                var examQuestions = new List<Examquestion>();
                int questionOrder = 1;

                foreach (var detail in matrixDetails)
                {
                    // Lấy random questions từ repository - đã lọc TopicId, QuestionType, IsActive
                    var matchingQuestions = await _unitOfWork.Questions.GetRandomQuestionsByFiltersAsync(
                        detail.TopicId!.Value, 
                        detail.QuestionType,
                        null, // Không filter theo level
                        detail.QuestionCount);

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

                // Lấy ExamQuestions - Dùng repository method
                var examQuestions = await _unitOfWork.ExamQuestions.GetByGeneratedExamIdAsync(generatedExamId);

                // Lấy Questions với Options - Dùng repository method
                var questionIds = examQuestions.Select(eq => eq.QuestionId).ToList();
                var questions = await _unitOfWork.Questions.GetQuestionsWithOptionsByIdsAsync(questionIds);

                var response = new GeneratedExamResponseDto
                {
                    GeneratedExamId = generatedExam.GeneratedExamId,
                    ExamRequestId = generatedExam.ExamRequestId,
                    Status = generatedExam.Status,
                    CreatedAt = generatedExam.CreatedAt,
                    ExamQuestions = examQuestions.Select((eq, index) =>
                    {
                        var question = questions.FirstOrDefault(q => q.QuestionId == eq.QuestionId);

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
                                QuestionText = question.QuestionText,
                                Explanation = question.Explanation,
                                Options = question.Options?.Select(o => new OptionDetailDto
                                {
                                    OptionId = o.OptionId,
                                    Answer = o.Answer,
                                    IsCorrect = o.IsCorrect ?? false
                                }).ToList() ?? new List<OptionDetailDto>()
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
        /// Export đề thi ra file Word (.docx) - Lấy cùng bộ câu hỏi đã generate
        /// </summary>
        public async Task<ApiResponse<byte[]>> ExportExamToWordAsync(Guid generatedExamId)
        {
            string? tempFilePath = null;

            try
            {
                _logger.LogInformation("[ExportExamToWord] Starting export for GeneratedExamId: {GeneratedExamId}", generatedExamId);

                // 1. Lấy GeneratedExam từ repository
                var generatedExam = await _unitOfWork.GeneratedExams.GetByIdAsync(generatedExamId);
                if (generatedExam == null)
                {
                    _logger.LogWarning("[ExportExamToWord] GeneratedExam not found: {GeneratedExamId}", generatedExamId);
                    return ApiResponse<byte[]>.ErrorResult($"Không tìm thấy đề thi với ID: {generatedExamId}");
                }

                // 2. Kiểm tra status - chỉ export khi READY
                if (generatedExam.Status != RoboChemistConstants.GENERATED_EXAM_STATUS_READY)
                {
                    _logger.LogWarning("[ExportExamToWord] GeneratedExam not ready. Status: {Status}", generatedExam.Status);
                    return ApiResponse<byte[]>.ErrorResult($"Đề thi chưa sẵn sàng để xuất. Trạng thái hiện tại: {generatedExam.Status}");
                }

                // 3. Lấy ExamRequest và Matrix từ repository
                var examRequest = await _unitOfWork.ExamRequests.GetByIdAsync(generatedExam.ExamRequestId);
                if (examRequest == null)
                {
                    _logger.LogError("[ExportExamToWord] ExamRequest not found for GeneratedExam: {GeneratedExamId}", generatedExamId);
                    return ApiResponse<byte[]>.ErrorResult("Không tìm thấy yêu cầu tạo đề");
                }

                var matrix = await _unitOfWork.Matrices.GetByIdAsync(examRequest.MatrixId);
                if (matrix == null)
                {
                    _logger.LogError("[ExportExamToWord] Matrix not found: {MatrixId}", examRequest.MatrixId);
                    return ApiResponse<byte[]>.ErrorResult("Không tìm thấy ma trận đề thi");
                }

                // 4. Lấy câu hỏi từ repository
                var examQuestions = await _unitOfWork.ExamQuestions.GetByGeneratedExamIdAsync(
                    generatedExamId, 
                    RoboChemistConstants.EXAMQUESTION_STATUS_ACTIVE
                );

                if (!examQuestions.Any())
                {
                    _logger.LogWarning("[ExportExamToWord] No questions found for GeneratedExam: {GeneratedExamId}", generatedExamId);
                    return ApiResponse<byte[]>.ErrorResult("Đề thi không có câu hỏi nào");
                }

                // 5. Lấy full Questions với Options từ repository
                var questionIds = examQuestions.Select(eq => eq.QuestionId).ToList();
                var questions = await _unitOfWork.Questions.GetQuestionsWithOptionsByIdsAsync(questionIds);

                // Sắp xếp questions theo thứ tự trong examQuestions
                var orderedQuestions = examQuestions
                    .Select(eq => questions.FirstOrDefault(q => q.QuestionId == eq.QuestionId))
                    .Where(q => q != null)
                    .Cast<Question>()
                    .ToList();

                _logger.LogInformation("[ExportExamToWord] Found {QuestionCount} questions", orderedQuestions.Count);

                // 6. Export to Word
                var wordBytes = await _wordExportService.ExportExamToWordAsync(
                    matrixName: matrix.Name ?? "Đề thi",
                    questions: orderedQuestions,
                    totalQuestions: matrix.TotalQuestion ?? orderedQuestions.Count,
                    timeLimit: null
                );

                // 7. Save to temp file
                tempFilePath = Path.Combine(Path.GetTempPath(), $"exam_{generatedExamId}_{DateTime.Now:yyyyMMddHHmmss}.docx");
                await File.WriteAllBytesAsync(tempFilePath, wordBytes);
                _logger.LogInformation("[ExportExamToWord] Saved to temp file: {TempFilePath}", tempFilePath);

                // 8. Upload to storage via TemplateService
                var fileName = $"{matrix.Name?.Replace(" ", "_") ?? "DeThi"}_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
                var uploadResponse = await _templateServiceClient.UploadFileAsync(
                    new FileStream(tempFilePath, FileMode.Open, FileAccess.Read),
                    fileName
                );

                // 9. Update export tracking information
                var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                Guid? exportedBy = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
                
                generatedExam.ExportedFileName = uploadResponse.ObjectKey;
                generatedExam.ExportedAt = DateTime.Now;
                generatedExam.ExportedBy = exportedBy;
                generatedExam.FileFormat = "docx";
                
                await _unitOfWork.GeneratedExams.UpdateAsync(generatedExam);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("[ExportExamToWord] SUCCESS - Exported and uploaded. ObjectKey: {ObjectKey}", uploadResponse.ObjectKey);
                
                return ApiResponse<byte[]>.SuccessResult(wordBytes, "Export đề thi thành công");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[ExportExamToWord] HTTP error during upload");
                return ApiResponse<byte[]>.ErrorResult($"Lỗi kết nối khi upload file: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ExportExamToWord] Unexpected error");
                return ApiResponse<byte[]>.ErrorResult($"Lỗi export đề thi: {ex.Message}");
            }
            finally
            {
                // Clean up temp file
                if (!string.IsNullOrEmpty(tempFilePath) && File.Exists(tempFilePath))
                {
                    try
                    {
                        File.Delete(tempFilePath);
                        _logger.LogInformation("[ExportExamToWord] Cleaned up temp file: {TempFilePath}", tempFilePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[ExportExamToWord] Failed to delete temp file: {TempFilePath}", tempFilePath);
                    }
                }
            }
        }

        /// <summary>
        /// Cập nhật trạng thái đề thi (PENDING -> READY -> EXPIRED)
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

                // Validate status hợp lệ - sử dụng constants
                var validStatuses = new[] 
                { 
                    RoboChemistConstants.GENERATED_EXAM_STATUS_PENDING,
                    RoboChemistConstants.GENERATED_EXAM_STATUS_READY,
                    RoboChemistConstants.GENERATED_EXAM_STATUS_EXPIRED
                };
                
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

                // Xóa các ExamQuestion liên quan - Dùng repository method
                var examQuestions = await _unitOfWork.ExamQuestions.GetByGeneratedExamIdAsync(generatedExamId);

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
