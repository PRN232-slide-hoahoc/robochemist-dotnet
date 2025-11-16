using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.ExamService.Model.Models;

namespace RoboChemist.ExamService.Repository.Interfaces
{
    public interface IGeneratedexamRepository : IGenericRepository<Generatedexam>
    {
        /// <summary>
        /// Lấy generated exams theo examRequestId
        /// </summary>
        Task<List<Generatedexam>> GetByExamRequestIdAsync(Guid examRequestId);
    }
}
