using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.ExamService.Model.Models;
using RoboChemist.ExamService.Repository.Interfaces;

namespace RoboChemist.ExamService.Repository.Implements
{
    public class GeneratedexamRepository : GenericRepository<Generatedexam>, IGeneratedexamRepository
    {
        public GeneratedexamRepository(DbContext context) : base(context)
        {
        }
    }
}
