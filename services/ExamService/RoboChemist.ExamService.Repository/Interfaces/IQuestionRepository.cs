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

        /// <summary>
        /// Lấy danh sách Questions với Options theo danh sách QuestionIds
        /// </summary>
        Task<List<Question>> GetQuestionsWithOptionsByIdsAsync(List<Guid> questionIds);

        /// <summary>
        /// Lấy danh sách Questions với filters (TopicId, search term)
        /// </summary>
        /// <param name="topicId">Lọc theo Topic (nullable)</param>
        /// <param name="search">Tìm kiếm trong QuestionText (nullable)</param>
        /// <returns>Danh sách câu hỏi với Options</returns>
        Task<List<Question>> GetQuestionsWithFiltersAsync(Guid? topicId, string? search);
    }
}
