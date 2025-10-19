using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.SlidesService.Model.Models;
using RoboChemist.SlidesService.Repository.Interfaces;

namespace RoboChemist.SlidesService.Repository.Implements
{
    public class SliderequestRepository : GenericRepository<Sliderequest>, ISliderequestRepository
    {
        public SliderequestRepository(DbContext context) : base(context)
        {
        }
    }
}
