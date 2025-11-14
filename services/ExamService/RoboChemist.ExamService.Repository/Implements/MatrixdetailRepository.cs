using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.ExamService.Model.Models;
using RoboChemist.ExamService.Repository.Interfaces;

namespace RoboChemist.ExamService.Repository.Implements
{
    public class MatrixdetailRepository : GenericRepository<Matrixdetail>, IMatrixdetailRepository
    {
        public MatrixdetailRepository(DbContext context) : base(context)
        {
        }

        /// <summary>
        /// Lấy tất cả MatrixDetails theo MatrixId
        /// </summary>
        public async Task<List<Matrixdetail>> GetByMatrixIdAsync(Guid matrixId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(md => md.MatrixId == matrixId)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy tất cả MatrixDetails active theo MatrixId
        /// </summary>
        public async Task<List<Matrixdetail>> GetActiveByMatrixIdAsync(Guid matrixId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(md => md.MatrixId == matrixId && md.IsActive == true)
                .ToListAsync();
        }
    }
}