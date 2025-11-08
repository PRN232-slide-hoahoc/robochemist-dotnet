using RoboChemist.Shared.Common.Constants;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.Shared.DTOs.FileDTOs;
using RoboChemist.Shared.DTOs.UserDTOs;
using RoboChemist.SlidesService.Model.Models;
using RoboChemist.SlidesService.Repository.Interfaces;
using RoboChemist.SlidesService.Service.HttpClients;
using RoboChemist.SlidesService.Service.Interfaces;
using static RoboChemist.Shared.DTOs.ExamServiceDTOs.ExamDTOs;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideRequestDTOs;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideResponseDTOs;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.WalletTransactionDTOs;

namespace RoboChemist.SlidesService.Service.Implements
{
    public class SlideService : ISlideService
    {
        private readonly IUnitOfWork _uow;
        private readonly IAuthServiceClient _authService;
        private readonly IGeminiService _geminiService;
        private readonly IPowerPointService _pptService;
        private readonly ITemplateServiceClient _templateService;
        private readonly IWalletServiceClient _walletService;

        public SlideService(IUnitOfWork uow, IAuthServiceClient authService, IGeminiService geminiService, IPowerPointService pptService, ITemplateServiceClient templateService, IWalletServiceClient walletService)
        {
            _uow = uow;
            _authService = authService;
            _geminiService = geminiService;
            _pptService = pptService;
            _templateService = templateService;
            _walletService = walletService;
        }
        public async Task<ApiResponse<SlideDto>> GenerateSlideAsync(GenerateSlideRequest request)
        {
			try
			{
                // Get user information
                UserDto? user = await _authService.GetCurrentUserAsync();
                if(user==null) 
                {
                    return ApiResponse<SlideDto>.ErrorResult("Người dùng không hợp lệ");
                }

                // Payment process
                CreateChangeBalanceRequestDto ceateChangeBalanceRequest = new()
                {
                    Amount = 15000,
                    ReferenceId = null
                };

                ApiResponse<CreateChangeBalanceRequestDto>? paymentResponse = await _walletService.CreatePaymentAsync(ceateChangeBalanceRequest);
                if (paymentResponse == null || !paymentResponse.Success)
                {
                    return ApiResponse<SlideDto>.ErrorResult(paymentResponse?.Message ?? "Không thể thực hiện thanh toán");
                }

                //Create slide request record
                Sliderequest slideReq = new()
                {
                    AiPrompt = request.AiPrompt,
                    NumberOfSlides = request.NumberOfSlides,
                    SyllabusId = request.SyllabusId,
                    TemplateId = request.TemplateId,
                    UserId = user.Id,
                    RequestedAt = DateTime.Now,
                    Status = RoboChemistConstants.SLIDEREQ_STATUS_PENDING
                };
                await _uow.Sliderequests.CreateAsync(slideReq);

                DataForGenerateSlideRequest reqData = await _uow.Sliderequests.GetDataRequestModelAsync(slideReq.Id);

                //Generate data
                ResponseGenerateDataDto? responseDto = await _geminiService.GenerateSlidesAsync(reqData);

                if (responseDto == null)
                {
                    slideReq.Status = RoboChemistConstants.SLIDEREQ_STATUS_FAILED;
                    await _uow.Sliderequests.UpdateAsync(slideReq);
                    return ApiResponse<SlideDto>.ErrorResult("Không thể tạo slide từ yêu cầu");
                }

                Generatedslide generatedSlide = new()
                {
                    JsonContent = string.Empty,
                    SlideRequestId = slideReq.Id,
                    GenerationStatus = RoboChemistConstants.GENSLIDE_STATUS_JSON,
                    FileFormat = RoboChemistConstants.File_Format_PPTX,
                };
                await _uow.Generatedslides.CreateAsync(generatedSlide);


                SlideFileInfomationDto fileInfo = await ProcessPptxFile(slideReq, responseDto);

                generatedSlide.FilePath = fileInfo.FilePath;
                generatedSlide.FileSize = fileInfo.FileSize;
                generatedSlide.SlideCount = fileInfo.SlideCount;
                generatedSlide.FileFormat = fileInfo.FileFormat ?? RoboChemistConstants.File_Format_PPTX;
                generatedSlide.GenerationStatus = RoboChemistConstants.GENSLIDE_STATUS_COMPLETED;
                generatedSlide.GeneratedAt = DateTime.Now;
                generatedSlide.ProcessingTime = (generatedSlide.GeneratedAt - slideReq.RequestedAt).Value.TotalSeconds;
                await _uow.Generatedslides.UpdateAsync(generatedSlide);

                slideReq.Status = RoboChemistConstants.SLIDEREQ_STATUS_COMPLETED;
                await _uow.Sliderequests.UpdateAsync(slideReq);

                SlideDto returnDto = new()
                {
                    GeneratedSlideId = generatedSlide.Id,
                    SlideRequestId = generatedSlide.SlideRequestId,
                    JsonContent = generatedSlide.JsonContent,
                    FileFormat = generatedSlide.FileFormat,
                    FilePath = generatedSlide.FilePath,
                    FileSize = generatedSlide.FileSize,
                    SlideCount = generatedSlide.SlideCount,
                    GenerationStatus = generatedSlide.GenerationStatus,
                    ProcessingTime = generatedSlide.ProcessingTime,
                    GeneratedAt = generatedSlide.GeneratedAt
                };
                return ApiResponse<SlideDto>.SuccessResult(returnDto);
            }
			catch (Exception ex)
			{
                return ApiResponse<SlideDto>.ErrorResult("Lỗi hệ thống", [ex.Message]);
            }
        }

        private async Task<SlideFileInfomationDto> ProcessPptxFile(Sliderequest slideReq, ResponseGenerateDataDto responseDto)
        {
            string outputFilePath = Path.Combine(Path.GetTempPath(), $"slide_{slideReq.Id}_{Guid.NewGuid()}.pptx");

            try
            {
                // Validate TemplateId
                if (!slideReq.TemplateId.HasValue)
                {
                    throw new InvalidOperationException("Template ID is required");
                }

                // Download template as stream from TemplateService
                (Stream templateStream, string contentType) = await _templateService.DownloadTemplateAsync(slideReq.TemplateId.Value);

                using (templateStream)
                {
                    // Import data to template and save to output file
                    _pptService.ImportDataToTemplate(responseDto, templateStream, outputFilePath);
                }

                // Upload generated PPTX file to storage
                FileUploadResponse uploadResponse;
                var fileName = (await _uow.Syllabuses.GetByIdAsync(slideReq.SyllabusId))!.Lesson;
                using (var fileStream = new FileStream(outputFilePath, FileMode.Open, FileAccess.Read))
                {
                    var generatedFileName = $"{fileName}.pptx";
                    uploadResponse = await _templateService.UploadFileAsync(fileStream, generatedFileName);
                }

                // Clean up temp file after successful upload
                if (File.Exists(outputFilePath))
                {
                    File.Delete(outputFilePath);
                }

                SlideFileInfomationDto returnInfo = new()
                {
                    FilePath = uploadResponse.ObjectKey,
                    FileSize = (int)uploadResponse.FileSize,
                    SlideCount = responseDto.ContentSlides.Count + 3,
                    FileFormat = RoboChemistConstants.File_Format_PPTX
                };

                return returnInfo;
            }
            catch (Exception ex)
            {
                // Clean up temp file if exists
                if (File.Exists(outputFilePath))
                {
                    File.Delete(outputFilePath);
                }
                throw new InvalidOperationException($"Failed to process PowerPoint file: {ex.Message}", ex);
            }
        }
    }
}
