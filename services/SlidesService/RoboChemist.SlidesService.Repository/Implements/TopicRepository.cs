using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.SlidesService.Model.Models;
using RoboChemist.SlidesService.Repository.Interfaces;
using static RoboChemist.Shared.DTOs.TopicDTOs.TopicDTOs;

namespace RoboChemist.SlidesService.Repository.Implements
{
    public class TopicRepository : GenericRepository<Topic>, ITopicRepository
    {
        public TopicRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<GetTopicDto>> GetFullTopicsAsync(Guid? gradeId)
        {
            List<GetTopicDto> topics = await _dbSet
                .Include(t => t.Grade)
                .Where(t => !gradeId.HasValue || t.GradeId == gradeId.Value)
                .Select(t => new GetTopicDto
                {
                    Id = t.Id,
                    GradeId = t.GradeId,
                    GradeName = t.Grade.GradeName,
                    SortOrder = t.SortOrder ?? 0,
                    Name = t.TopicName,
                    Description = t.Description ?? string.Empty
                })
                .OrderBy(t => t.GradeName)
                .ThenBy(t => t.SortOrder)
                .ToListAsync();

            return topics;
        }
    }
}
