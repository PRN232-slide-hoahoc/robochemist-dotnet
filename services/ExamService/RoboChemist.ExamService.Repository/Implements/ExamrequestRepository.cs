using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.ExamService.Model.Models;
using RoboChemist.ExamService.Repository.Interfaces;

namespace RoboChemist.ExamService.Repository.Implements
{
    public class ExamrequestRepository : GenericRepository<Examrequest>, IExamrequestRepository
    {
        public ExamrequestRepository(DbContext context) : base(context)
        {
        }
    }
}
