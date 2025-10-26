using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.ExamService.Model.Models;
using RoboChemist.ExamService.Repository.Interfaces;

namespace RoboChemist.ExamService.Repository.Implements
{
    public class OptionRepository : GenericRepository<Option>, IOptionRepository
    {
        public OptionRepository(DbContext context) : base(context)
        {
        }
    }
}
