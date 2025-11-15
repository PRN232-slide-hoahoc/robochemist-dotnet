using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.SlidesService.Model.Models;
using RoboChemist.SlidesService.Repository.Interfaces;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideRequestDTOs;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideResponseDTOs;

namespace RoboChemist.SlidesService.Repository.Implements
{
    public class GeneratedslideRepository : GenericRepository<Generatedslide>, IGeneratedslideRepository
    {
        public GeneratedslideRepository(DbContext context) : base(context)
        {
        }

        public async Task<(List<SlideDetailDto> Slides, int TotalCount)> GetSlidesPaginatedAsync(Guid userId, GetSlidesRequest request, bool isAdmin = false)
        {
            // Base query with all necessary includes
            var query = _context.Set<Generatedslide>()
                .Where(gs => isAdmin || gs.SlideRequest.UserId == userId)
                .Include(gs => gs.SlideRequest)
                    .ThenInclude(sr => sr.Syllabus)
                        .ThenInclude(s => s.Topic)
                            .ThenInclude(t => t.Grade)
                .AsQueryable();

            // Apply filters
            if (request.GradeId.HasValue)
            {
                query = query.Where(gs => gs.SlideRequest.Syllabus.Topic.GradeId == request.GradeId.Value);
            }

            if (request.TopicId.HasValue)
            {
                query = query.Where(gs => gs.SlideRequest.Syllabus.TopicId == request.TopicId.Value);
            }

            if (!string.IsNullOrEmpty(request.GenerationStatus))
            {
                query = query.Where(gs => gs.GenerationStatus == request.GenerationStatus);
            }

            // Get total count after filters
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "gradename" => request.SortOrder?.ToLower() == "asc"
                    ? query.OrderBy(gs => gs.SlideRequest.Syllabus.Topic.Grade.GradeName)
                    : query.OrderByDescending(gs => gs.SlideRequest.Syllabus.Topic.Grade.GradeName),

                "topicsortorder" => request.SortOrder?.ToLower() == "asc"
                    ? query.OrderBy(gs => gs.SlideRequest.Syllabus.Topic.SortOrder)
                    : query.OrderByDescending(gs => gs.SlideRequest.Syllabus.Topic.SortOrder),

                "lessonorder" => request.SortOrder?.ToLower() == "asc"
                    ? query.OrderBy(gs => gs.SlideRequest.Syllabus.LessonOrder)
                    : query.OrderByDescending(gs => gs.SlideRequest.Syllabus.LessonOrder),

                "generatedat" or _ => request.SortOrder?.ToLower() == "asc"
                    ? query.OrderBy(gs => gs.GeneratedAt)
                    : query.OrderByDescending(gs => gs.GeneratedAt)
            };

            // Apply pagination and projection
            var slides = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(gs => new SlideDetailDto
                {
                    GeneratedSlideId = gs.Id,
                    SlideRequestId = gs.SlideRequestId,
                    FileFormat = gs.FileFormat,
                    FilePath = gs.FilePath,
                    FileSize = gs.FileSize,
                    SlideCount = gs.SlideCount,
                    GenerationStatus = gs.GenerationStatus,
                    ProcessingTime = gs.ProcessingTime,
                    GeneratedAt = gs.GeneratedAt,

                    // Slide Request Information
                    NumberOfSlides = gs.SlideRequest.NumberOfSlides,
                    AiPrompt = gs.SlideRequest.AiPrompt,
                    RequestStatus = gs.SlideRequest.Status,
                    RequestedAt = gs.SlideRequest.RequestedAt,

                    // Syllabus Information
                    SyllabusId = gs.SlideRequest.SyllabusId,
                    SyllabusLesson = gs.SlideRequest.Syllabus.Lesson,
                    LearningObjectives = gs.SlideRequest.Syllabus.LearningObjectives,
                    LessonOrder = gs.SlideRequest.Syllabus.LessonOrder,

                    // Topic Information
                    TopicId = gs.SlideRequest.Syllabus.TopicId,
                    TopicName = gs.SlideRequest.Syllabus.Topic.TopicName,
                    TopicSortOrder = gs.SlideRequest.Syllabus.Topic.SortOrder,

                    // Grade Information
                    GradeId = gs.SlideRequest.Syllabus.Topic.GradeId,
                    GradeName = gs.SlideRequest.Syllabus.Topic.Grade.GradeName
                })
                .ToListAsync();

            return (slides, totalCount);
        }

        public async Task<Generatedslide?> GetSlideWithDetailsAsync(Guid slideId)
        {
            return await _context.Set<Generatedslide>()
                .Include(gs => gs.SlideRequest)
                    .ThenInclude(sr => sr.Syllabus)
                .FirstOrDefaultAsync(gs => gs.Id == slideId);
        }
    }
}
