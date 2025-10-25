using static RoboChemist.Shared.DTOs.SlideDTOs.SlideRequestDTOs;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideResponseDTOs;

namespace RoboChemist.SlidesService.Service.Interfaces
{
    public interface IGeminiService
    {
        Task<ResponseGenerateDataDto?> GenerateSlidesAsync(DataForGenerateSlideRequest request);
    }
}
