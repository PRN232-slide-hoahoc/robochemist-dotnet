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

        /// <summary>
        /// Lấy exam requests theo userId và status, sắp xếp theo CreatedAt giảm dần
        /// </summary>
        public async Task<List<Examrequest>> GetExamRequestsByUserAsync(Guid userId, string? status = null)
        {
            var query = _context.Set<Examrequest>().Where(er => er.UserId == userId);

            // Lọc theo status nếu có
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(er => er.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            // Sắp xếp theo CreatedAt giảm dần
            return await query.OrderByDescending(er => er.CreatedAt).ToListAsync();
        }
    }
}
