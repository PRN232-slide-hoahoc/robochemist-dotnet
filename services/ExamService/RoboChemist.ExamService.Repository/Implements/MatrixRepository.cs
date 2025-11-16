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

        /// <summary>
        /// Lấy ma trận theo user và trạng thái, sắp xếp theo CreatedAt giảm dần
        /// </summary>
        public async Task<List<Matrix>> GetMatricesByUserAsync(Guid? userId, bool? isActive = null)
        {
            var query = _context.Set<Matrix>().AsQueryable();

            // Lọc theo userId
            if (userId.HasValue)
            {
                query = query.Where(m => m.CreatedBy == userId.Value);
            }

            // Lọc theo isActive
            if (isActive.HasValue)
            {
                query = query.Where(m => m.IsActive == isActive.Value);
            }

            // Sắp xếp theo CreatedAt giảm dần (mới nhất lên đầu)
            return await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
        }

        /// <summary>
        /// Lấy tất cả ma trận theo trạng thái, sắp xếp theo CreatedAt giảm dần
        /// </summary>
        public async Task<List<Matrix>> GetMatricesByStatusAsync(bool? isActive = null)
        {
            var query = _context.Set<Matrix>().AsQueryable();

            // Lọc theo isActive
            if (isActive.HasValue)
            {
                query = query.Where(m => m.IsActive == isActive.Value);
            }

            // Sắp xếp theo CreatedAt giảm dần (mới nhất lên đầu)
            return await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
        }
    }
}
