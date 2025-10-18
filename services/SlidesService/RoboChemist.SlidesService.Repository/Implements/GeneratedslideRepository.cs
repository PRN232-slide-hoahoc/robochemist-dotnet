using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.SlidesService.Model.Models;
using RoboChemist.SlidesService.Repository.Interfaces;

namespace RoboChemist.SlidesService.Repository.Implements
{
    public class GeneratedslideRepository : GenericRepository<Generatedslide>, IGeneratedslideRepository
    {
        public GeneratedslideRepository(DbContext context) : base(context)
        {
        }
    }
}
