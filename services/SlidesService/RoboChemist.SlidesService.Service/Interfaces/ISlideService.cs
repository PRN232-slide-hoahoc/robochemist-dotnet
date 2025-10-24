using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideRequestDTOs;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideResponseDTOs;

namespace RoboChemist.SlidesService.Service.Interfaces
{
    public interface ISlideService
    {
        Task<ApiResponse<SlideDto>> GenerateSlideAsync(GenerateSlideRequest request);
    }
}
