using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.ExamService.Model.Models;

namespace RoboChemist.ExamService.Repository.Interfaces
{
    public interface IMatrixRepository : IGenericRepository<Matrix>
    {
        /// <summary>
        /// Lấy ma trận theo user và trạng thái, có sắp xếp
        /// </summary>
        Task<List<Matrix>> GetMatricesByUserAsync(Guid? userId, bool? isActive = null);
        
        /// <summary>
        /// Lấy tất cả ma trận theo trạng thái, có sắp xếp
        /// </summary>
        Task<List<Matrix>> GetMatricesByStatusAsync(bool? isActive = null);
    }
}
