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

        /// <summary>
        /// Lấy danh sách Questions với Options theo danh sách QuestionIds
        /// </summary>
        public async Task<List<Question>> GetQuestionsWithOptionsByIdsAsync(List<Guid> questionIds)
        {
            return await _context.Set<Question>()
                .Include(q => q.Options)
                .Where(q => questionIds.Contains(q.QuestionId))
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách Questions với filters (TopicId, search term)
        /// NOTE: Query logic phải ở Repository, KHÔNG ĐƯỢC query ở Service layer
        /// </summary>
        public async Task<List<Question>> GetQuestionsWithFiltersAsync(Guid? topicId, string? search)
        {
            var query = _context.Set<Question>()
                .Include(q => q.Options)
                .AsQueryable();

            // Filter by TopicId
            if (topicId.HasValue)
            {
                query = query.Where(q => q.TopicId == topicId.Value);
            }

            // Search in QuestionText
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(q => q.QuestionText.Contains(search));
            }

            return await query
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }
    }
}