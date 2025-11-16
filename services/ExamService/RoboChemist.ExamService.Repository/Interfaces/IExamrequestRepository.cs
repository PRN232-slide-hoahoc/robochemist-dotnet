using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.ExamService.Model.Models;

namespace RoboChemist.ExamService.Repository.Interfaces
{
    public interface IExamrequestRepository : IGenericRepository<Examrequest>
    {
        /// <summary>
        /// Lấy exam requests theo userId và status, sắp xếp theo CreatedAt giảm dần
        /// </summary>
        Task<List<Examrequest>> GetExamRequestsByUserAsync(Guid userId, string? status = null);
    }
}
