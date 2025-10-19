using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.SlidesService.Model.Models;
using RoboChemist.SlidesService.Repository.Interfaces;
using static RoboChemist.Shared.DTOs.SyllabusDTOs.SyllabusResponseDTOs;

namespace RoboChemist.SlidesService.Repository.Implements
{
    public class SyllabusRepository : GenericRepository<Syllabus>, ISyllabusRepository
    {
        public SyllabusRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<GetSyllabusDto>> GetFullInformationAsync(Guid? gradeId, Guid? topicId)
        {
            List<GetSyllabusDto> syllabusDtos = await _dbSet
                .Include(s => s.Topic)
                .Include(t => t.Topic.Grade)
                .Where(t => !gradeId.HasValue || t.Topic.GradeId == gradeId.Value)
                .Where(t => !topicId.HasValue || t.TopicId == topicId.Value)
                .Select(s => new GetSyllabusDto
                {
                    Id = s.Id,
                    GradeId = s.Topic.GradeId,
                    GradeName = s.Topic.Grade.GradeName,
                    TopicId = s.TopicId,
                    TopicOrder = s.Topic.SortOrder,
                    TopicName = s.Topic.TopicName,
                    LessonOrder = s.LessonOrder,
                    Lesson = s.Lesson,
                    LearningObjectives = s.LearningObjectives,
                    ContentOutline = s.ContentOutline,
                    KeyConcepts = s.KeyConcepts,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    IsActive = s.IsActive,
                })
                .OrderBy(s => s.GradeName)
                .ThenBy(s => s.TopicOrder)
                .ThenBy(s => s.LessonOrder)
                .ToListAsync();

            return syllabusDtos;
        }
    }
}
