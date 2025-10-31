using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.ExamService.Model.Models;

namespace RoboChemist.ExamService.Repository.Interfaces
{
    public interface IQuestionRepository : IGenericRepository<Question>
    {
        /// <summary>
        /// Lấy danh sách Questions theo TopicId
        /// </summary>
        Task<List<Question>> GetByTopicIdAsync(Guid topicId);

        /// <summary>
        /// Lấy danh sách Questions theo Status (IsActive)
        /// </summary>
        Task<List<Question>> GetByStatusAsync(bool isActive);

        /// <summary>
        /// Lấy danh sách Questions theo TopicId và Status
        /// </summary>
        Task<List<Question>> GetByTopicIdAndStatusAsync(Guid topicId, bool isActive);
    }
}
