using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.ExamService.Model.Models;
using RoboChemist.ExamService.Repository.Interfaces;

namespace RoboChemist.ExamService.Repository.Implements
{
    public class ExamquestionRepository : GenericRepository<Examquestion>, IExamquestionRepository
    {
        public ExamquestionRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<Examquestion>> GetByGeneratedExamIdAsync(Guid generatedExamId, string? status = null)
        {
            var query = _dbSet.Where(eq => eq.GeneratedExamId == generatedExamId);
            
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(eq => eq.Status == status);
            }

            return await query.OrderBy(eq => eq.CreatedAt).ToListAsync();
        }
    }
}
