using RoboChemist.Shared.DTOs.Common;
using RoboChemist.SlidesService.Model.Models;
using RoboChemist.SlidesService.Repository.Interfaces;
using RoboChemist.SlidesService.Service.Interfaces;
using static RoboChemist.Shared.DTOs.SyllabusDTOs.SyllabusRequestDTOs;
using static RoboChemist.Shared.DTOs.SyllabusDTOs.SyllabusResponseDTOs;

namespace RoboChemist.SlidesService.Service.Implements
{
    public class SyllabusService : ISyllabusService
    {
        private readonly IUnitOfWork _uow;
        
        public SyllabusService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ApiResponse<SyllabusDto>> CreateSyllabusAsync(CreateSyllabusRequestDto request)
        {
            try
            {
                Topic? topic = await _uow.Topics.GetByIdAsync(request.TopicId);
                if (topic == null)
                    return ApiResponse<SyllabusDto>.ErrorResult("Chủ đề không tồn tại");

                Syllabus syllabus = new()
                {
                    TopicId = request.TopicId,
                    LessonOrder = request.LessonOrder,
                    Lesson = request.Lesson,
                    LearningObjectives = request.LearningObjectives,
                    KeyConcepts = request.KeyConcepts,
                    ContentOutline = request.ContentOutline,
                    CreatedAt = DateTime.Now,
                    IsActive = true,
                };

                await _uow.Syllabuses.CreateAsync(syllabus);

                SyllabusDto syllabusDto = await _uow.Syllabuses.GetDtoByIdAsync(syllabus.Id);

                return ApiResponse<SyllabusDto>.SuccessResult(syllabusDto);

            }
            catch (Exception)
            {
                return ApiResponse<SyllabusDto>.ErrorResult("Lỗi hệ thống");
            }
        }

        public async Task<ApiResponse<List<SyllabusDto>>> GetSyllabusesAsync(Guid? gradeId, Guid? topicId)
        {
            try
            {
                var gradeTask = gradeId.HasValue ? _uow.Grades.GetByIdAsync(gradeId.Value) : Task.FromResult<Grade?>(null);
                var topicTask = topicId.HasValue ? _uow.Topics.GetByIdAsync(topicId.Value) : Task.FromResult<Topic?>(null);

                await Task.WhenAll(gradeTask, topicTask);

                if (gradeId.HasValue && gradeTask.Result == null)
                    return ApiResponse<List<SyllabusDto>>.ErrorResult("Khối lớp không tồn tại");

                if (topicId.HasValue && topicTask.Result == null)
                    return ApiResponse<List<SyllabusDto>>.ErrorResult("Chủ đề không tồn tại");

                var syllabuses = await _uow.Syllabuses.GetFullInformationAsync(gradeId, topicId);
                return ApiResponse<List<SyllabusDto>>.SuccessResult(syllabuses);
            }
            catch (Exception)
            {
                return ApiResponse<List<SyllabusDto>>.ErrorResult("Lỗi hệ thống");
            }
        }

        public async Task<ApiResponse<SyllabusDto>> GetSyllabusAsync(Guid id)
        {
            try
            {
                Syllabus? syllabus = await _uow.Syllabuses.GetByIdAsync(id);

                if (syllabus == null)
                    return ApiResponse<SyllabusDto>.ErrorResult("Giáo trình không tồn tại");

                SyllabusDto syllabusDto = await _uow.Syllabuses.GetDtoByIdAsync(syllabus.Id);

                return ApiResponse<SyllabusDto>.SuccessResult(syllabusDto);
            }
            catch (Exception)
            {
                return ApiResponse<SyllabusDto>.ErrorResult("Lỗi hệ thống");
            }
        }

        public async Task<ApiResponse<SyllabusDto>> UpdateSyllabusAsync(Guid id, CreateSyllabusRequestDto request)
        {
            try
            {
                Topic? topic = await _uow.Topics.GetByIdAsync(request.TopicId);
                if (topic == null)
                    return ApiResponse<SyllabusDto>.ErrorResult("Chủ đề không tồn tại");

                Syllabus? syllabus = await _uow.Syllabuses.GetByIdAsync(id);

                if (syllabus == null)
                    return ApiResponse<SyllabusDto>.ErrorResult("Giáo trình không tồn tại");

                syllabus.TopicId = request.TopicId;
                syllabus.LessonOrder = request.LessonOrder;
                syllabus.Lesson = request.Lesson;
                syllabus.LearningObjectives = request.LearningObjectives;
                syllabus.KeyConcepts = request.KeyConcepts;
                syllabus.ContentOutline = request.ContentOutline;
                syllabus.UpdatedAt = DateTime.Now;

                await _uow.Syllabuses.UpdateAsync(syllabus);

                SyllabusDto syllabusDto = await _uow.Syllabuses.GetDtoByIdAsync(syllabus.Id);

                return ApiResponse<SyllabusDto>.SuccessResult(syllabusDto);

            }
            catch (Exception)
            {
                return ApiResponse<SyllabusDto>.ErrorResult("Lỗi hệ thống");
            }
        }

        public async Task<ApiResponse<bool>> ToggleSyllabusStatusAsync(Guid id)
        {
            try
            {
                Syllabus? syllabus = await _uow.Syllabuses.GetByIdAsync(id);

                if (syllabus == null)
                    return ApiResponse<bool>.ErrorResult("Giáo trình không tồn tại");

                syllabus.IsActive = !syllabus.IsActive;

                await _uow.Syllabuses.UpdateAsync(syllabus);

                return ApiResponse<bool>.SuccessResult( (bool) syllabus.IsActive!);
            }
            catch (Exception)
            {
                return ApiResponse<bool>.ErrorResult("Lỗi hệ thống");
            }
        }
    }
}
