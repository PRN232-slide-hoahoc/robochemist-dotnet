using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.SlidesService.Model.Models;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideRequestDTOs;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideResponseDTOs;

namespace RoboChemist.SlidesService.Repository.Interfaces
{
    public interface IGeneratedslideRepository : IGenericRepository<Generatedslide>
    {
        /// <summary>
        /// Get paginated slides for a specific user with filters and sorting
        /// </summary>
        /// <param name="userId">User unique identifier</param>
        /// <param name="request">Request containing pagination, filter, and sort parameters</param>
        /// <returns>Tuple containing list of SlideDetailDtos and total count</returns>
        Task<(List<SlideDetailDto> Slides, int TotalCount)> GetSlidesPaginatedAsync(Guid userId, GetSlidesRequest request, bool isAdmin = false);

        /// <summary>
        /// Get slide with related data by slide ID
        /// </summary>
        /// <param name="slideId">Generated slide ID</param>
        /// <returns>Generatedslide entity with navigation properties loaded</returns>
        Task<Generatedslide?> GetSlideWithDetailsAsync(Guid slideId);
    }
}
