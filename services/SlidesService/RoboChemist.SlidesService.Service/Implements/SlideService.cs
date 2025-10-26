using RoboChemist.Shared.Common.Constants;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.Shared.DTOs.UserDTOs;
using RoboChemist.SlidesService.Model.Models;
using RoboChemist.SlidesService.Repository.Interfaces;
using RoboChemist.SlidesService.Service.Interfaces;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideRequestDTOs;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideResponseDTOs;

namespace RoboChemist.SlidesService.Service.Implements
{
    public class SlideService : ISlideService
    {
        private readonly IUnitOfWork _uow;
        private readonly IAuthServiceClient _authService;
        private readonly IGeminiService _geminiService;
        private readonly IPowerPointService _pptService;

        public SlideService(IUnitOfWork uow, IAuthServiceClient authService, IGeminiService geminiService, IPowerPointService pptService)
        {
            _uow = uow;
            _authService = authService;
            _geminiService = geminiService;
            _pptService = pptService;
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
            string returnFilePath = string.Empty;

            // Tải pptx
            _pptService.ImportDataToTemplate(responseDto, @"C:\Users\Admin\Downloads\template.pptx", @"C:\Users\Admin\Downloads\doneeee.pptx");
            // Upload pptx

            SlideFileInfomationDto returnInfo = new();

            return returnInfo;
        }
    }
}
