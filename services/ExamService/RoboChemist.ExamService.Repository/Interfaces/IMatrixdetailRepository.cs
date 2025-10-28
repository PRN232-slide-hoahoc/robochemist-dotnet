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
    }
}
