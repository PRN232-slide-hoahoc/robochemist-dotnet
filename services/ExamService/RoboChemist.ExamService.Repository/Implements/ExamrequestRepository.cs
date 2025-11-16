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

        public async Task<List<Examrequest>> GetExamRequestsByUserAsync(Guid userId, string? status = null)
        {
            var query = _dbSet.Where(er => er.UserId == userId);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(er => er.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            return await query.OrderByDescending(er => er.CreatedAt).ToListAsync();
        }
    }
}
