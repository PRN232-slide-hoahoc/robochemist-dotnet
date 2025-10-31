using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.ExamService.Model.Models;
using RoboChemist.ExamService.Repository.Interfaces;

namespace RoboChemist.ExamService.Repository.Implements
{
    public class QuestionRepository : GenericRepository<Question>, IQuestionRepository
    {
        public QuestionRepository(DbContext context) : base(context)
        {
        }

        /// <summary>
        /// Lấy danh sách Questions theo TopicId
        /// </summary>
        public async Task<List<Question>> GetByTopicIdAsync(Guid topicId)
        {
            return await _context.Set<Question>()
                .Where(q => q.TopicId == topicId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách Questions theo Status (IsActive)
        /// </summary>
        public async Task<List<Question>> GetByStatusAsync(bool isActive)
        {
            return await _context.Set<Question>()
                .Where(q => q.IsActive == isActive)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách Questions theo TopicId và Status
        /// </summary>
        public async Task<List<Question>> GetByTopicIdAndStatusAsync(Guid topicId, bool isActive)
        {
            return await _context.Set<Question>()
                .Where(q => q.TopicId == topicId && q.IsActive == isActive)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }
    }
}
