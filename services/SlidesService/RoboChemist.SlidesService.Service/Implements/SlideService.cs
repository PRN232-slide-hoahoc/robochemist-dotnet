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

        public SlideService(IUnitOfWork uow, IAuthServiceClient authService)
        {
            _uow = uow;
            _authService = authService;
        }
        public async Task<ApiResponse<SlideDto>> GenerateSlideAsync(GenerateSlideRequest request)
        {
			try
			{
                UserDto? user = await _authService.GetCurrentUserAsync();
                if(user==null) 
                {
                    return ApiResponse<SlideDto>.ErrorResult("Người dùng không hợp lệ");
                }

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

                string responseData = await GetGeneratedDataAsync(reqData);

                Generatedslide generatedSlide = new()
                {
                    JsonContent = responseData,
                    SlideRequestId = slideReq.Id,
                    GenerationStatus = RoboChemistConstants.GENSLIDE_STATUS_JSON,
                    FileFormat = RoboChemistConstants.File_Format_PPTX,
                };
                await _uow.Generatedslides.CreateAsync(generatedSlide);

                ResponseGenerateDataDto responseDto = ConvertJsonToModel(responseData);

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
			catch (Exception)
			{
                return ApiResponse<SlideDto>.ErrorResult("Lỗi hệ thống");
            }
        }

        private async Task<string> GetGeneratedDataAsync(DataForGenerateSlideRequest reqData)
        {
            return "HARDDDDDDDDDDDD";
        }

        private static ResponseGenerateDataDto ConvertJsonToModel(string json)
        {
            ResponseGenerateDataDto response = new();

            return response;
        }

        private async Task<SlideFileInfomationDto> ProcessPptxFile(Sliderequest slideReq, ResponseGenerateDataDto responseDto)
        {
            string reuturnFilePath = string.Empty;

            // Tai pptx
            ImportDataToTemplate(responseDto); // Import data
            // Upload pptx

            SlideFileInfomationDto returnInfo = new();

            return returnInfo;
        }

        private static void ImportDataToTemplate(ResponseGenerateDataDto responseDto) 
        { 
        }
    }
}
