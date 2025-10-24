using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.SlidesService.Model.Models;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideRequestDTOs;

namespace RoboChemist.SlidesService.Repository.Interfaces
{
    public interface ISliderequestRepository : IGenericRepository<Sliderequest>
    {
        Task<DataForGenerateSlideRequest> GetDataRequestModelAsync(Guid id);
    }
}
