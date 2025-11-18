using static RoboChemist.Shared.DTOs.SlideDTOs.SlideRequestDTOs;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideResponseDTOs;

namespace RoboChemist.SlidesService.Service.Interfaces
{
    public interface IGeminiService
    {
        /// <summary>
        /// Call Google Gemini AI to generate structured JSON slide content including FirstSlide, TableOfContentSlide, 
        /// and ContentSlides with hierarchical bullet points (Level 1-3). Validates structure and content length for PowerPoint display
        /// </summary>
        /// <param name="request">Input data including lesson name, topic, grade, learning objectives, content outline, key concepts, desired slide count, and custom AI prompt</param>
        /// <returns>ResponseGenerateDataDto containing complete slide structure, or null if AI returns invalid data or JSON parsing fails</returns>
        Task<ResponseGenerateDataDto?> GenerateSlidesAsync(DataForGenerateSlideRequest request);
    }
}
