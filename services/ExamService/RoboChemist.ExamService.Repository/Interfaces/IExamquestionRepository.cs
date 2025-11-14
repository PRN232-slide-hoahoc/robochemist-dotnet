using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.ExamService.Model.Models;

namespace RoboChemist.ExamService.Repository.Interfaces
{
    public interface IExamquestionRepository : IGenericRepository<Examquestion>
    {
        /// <summary>
        /// Lấy danh sách ExamQuestion theo GeneratedExamId và status
        /// </summary>
        Task<List<Examquestion>> GetByGeneratedExamIdAsync(Guid generatedExamId, string? status = null);
    }
}
