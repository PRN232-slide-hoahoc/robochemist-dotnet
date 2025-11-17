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

        public async Task<List<Question>> GetByTopicIdAsync(Guid topicId)
        {
            return await _dbSet
                .Where(q => q.TopicId == topicId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Question>> GetByStatusAsync(bool isActive)
        {
            return await _dbSet
                .Where(q => q.IsActive == isActive)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Question>> GetByTopicIdAndStatusAsync(Guid topicId, bool isActive)
        {
            return await _dbSet
                .Where(q => q.TopicId == topicId && q.IsActive == isActive)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Question>> GetQuestionsWithOptionsByIdsAsync(List<Guid> questionIds)
        {
            return await _dbSet
                .Include(q => q.Options)
                .Where(q => questionIds.Contains(q.QuestionId))
                .ToListAsync();
        }

        public async Task<List<Question>> GetQuestionsWithFiltersAsync(Guid? topicId, string? search, string? level)
        {
            var query = _dbSet
                .Include(q => q.Options)
                .AsQueryable();

            if (topicId.HasValue)
            {
                query = query.Where(q => q.TopicId == topicId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(q => q.QuestionText.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(level))
            {
                query = query.Where(q => q.Level == level);
            }

            return await query
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Question>> GetRandomQuestionsByFiltersAsync(Guid topicId, string questionType, string? level, int count)
        {
            var query = _dbSet
                .Where(q => q.TopicId == topicId 
                         && q.QuestionType == questionType 
                         && q.IsActive == true);

            if (!string.IsNullOrEmpty(level))
            {
                query = query.Where(q => q.Level == level);
            }

            return await query
                .OrderBy(x => Guid.NewGuid())
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> CountQuestionsByFiltersAsync(Guid topicId, string questionType, string? level = null, bool isActive = true)
        {
            var query = _dbSet.Where(q => 
                q.TopicId == topicId && 
                q.QuestionType == questionType &&
                q.IsActive == isActive
            );

            if (!string.IsNullOrEmpty(level))
            {
                query = query.Where(q => q.Level == level);
            }

            return await query.CountAsync();
        }

        public async Task<Question?> GetQuestionsByIdsAsync(Guid id)
        {
            return await _dbSet
                .Include(q => q.Options)
                .Where(q => q.QuestionId == id)
                .FirstOrDefaultAsync();
        }
    }
}