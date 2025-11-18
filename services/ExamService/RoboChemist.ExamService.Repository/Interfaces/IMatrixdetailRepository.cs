using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.ExamService.Model.Models;

namespace RoboChemist.ExamService.Repository.Interfaces
{
    public interface IMatrixdetailRepository : IGenericRepository<Matrixdetail>
    {
        /// <summary>
        /// Lấy tất cả MatrixDetails theo MatrixId
        /// </summary>
        Task<List<Matrixdetail>> GetByMatrixIdAsync(Guid matrixId);

        /// <summary>
        /// Lấy tất cả MatrixDetails active theo MatrixId
        /// </summary>
        Task<List<Matrixdetail>> GetActiveByMatrixIdAsync(Guid matrixId);
    }
}
