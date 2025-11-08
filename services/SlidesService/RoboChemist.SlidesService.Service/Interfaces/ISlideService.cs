using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideRequestDTOs;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideResponseDTOs;

namespace RoboChemist.SlidesService.Service.Interfaces
{
    public interface ISlideService
    {
        /// <summary>
        /// Generate PowerPoint slides by authenticating user, processing payment, calling AI (Gemini) to generate JSON content,
        /// applying content to PowerPoint template, and uploading PPTX file to storage
        /// </summary>
        /// <param name="request">Slide generation request including AI prompt, number of slides, syllabus ID, and template ID</param>
        /// <returns>ApiResponse containing SlideDto with slide ID, file path, size, slide count, generation status, and processing time</returns>
        Task<ApiResponse<SlideDto>> GenerateSlideAsync(GenerateSlideRequest request);
    }
}
