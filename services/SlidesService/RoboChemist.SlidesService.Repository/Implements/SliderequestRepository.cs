using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.SlidesService.Model.Models;
using RoboChemist.SlidesService.Repository.Interfaces;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideRequestDTOs;

namespace RoboChemist.SlidesService.Repository.Implements
{
    public class SliderequestRepository : GenericRepository<Sliderequest>, ISliderequestRepository
    {
        public SliderequestRepository(DbContext context) : base(context)
        {
        }

        public async Task<DataForGenerateSlideRequest> GetDataRequestModelAsync(Guid id)
        {
            DataForGenerateSlideRequest model = (await _dbSet
                .Where(sr => sr.Id == id)
                .Include(sr => sr.Syllabus)
                    .ThenInclude(sy => sy.Topic)
                    .ThenInclude(t => t.Grade)
                .Select(sr => new DataForGenerateSlideRequest
                {
                    AiPrompt = sr.AiPrompt,
                    NumberOfSlides = sr.NumberOfSlides,
                    ContentOutline = sr.Syllabus.ContentOutline,
                    GradeName = sr.Syllabus.Topic.Grade.GradeName,
                    KeyConcepts = sr.Syllabus.KeyConcepts,
                    Lesson = sr.Syllabus.Lesson,
                    LessonOrder = sr.Syllabus.LessonOrder,
                    TopicName = sr.Syllabus.Topic.TopicName,
                    TopicOrder = sr.Syllabus.Topic.SortOrder,
                    LearningObjectives = sr.Syllabus.LearningObjectives
                })
                .FirstOrDefaultAsync())!;

            return model;
        }
    }
}
