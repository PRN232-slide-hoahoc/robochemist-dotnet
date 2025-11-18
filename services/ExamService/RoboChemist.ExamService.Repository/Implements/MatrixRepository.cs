using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.ExamService.Model.Models;
using RoboChemist.ExamService.Repository.Interfaces;

namespace RoboChemist.ExamService.Repository.Implements
{
    public class MatrixRepository : GenericRepository<Matrix>, IMatrixRepository
    {
        public MatrixRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<Matrix>> GetMatricesByUserAsync(Guid? userId, bool? isActive = null)
        {
            var query = _dbSet.AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(m => m.CreatedBy == userId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(m => m.IsActive == isActive.Value);
            }

            return await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
        }

        public async Task<List<Matrix>> GetMatricesByStatusAsync(bool? isActive = null)
        {
            var query = _dbSet.AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(m => m.IsActive == isActive.Value);
            }

            return await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
        }
    }
}
