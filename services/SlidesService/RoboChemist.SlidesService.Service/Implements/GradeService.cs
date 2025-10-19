using RoboChemist.Shared.DTOs.Common;
using RoboChemist.SlidesService.Model.Models;
using RoboChemist.SlidesService.Repository.Interfaces;
using RoboChemist.SlidesService.Service.Interfaces;
using static RoboChemist.Shared.DTOs.GradeDTOs.GradeDTOs;

namespace RoboChemist.SlidesService.Service.Implements
{
    public class GradeService : IGradeService
    {
        private readonly IUnitOfWork _uow;

        public GradeService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ApiResponse<List<GradeDto>>> GetGradesAsync()
        {
            try
            {
                List<Grade> grades = await _uow.Grades.GetAllAsync();
                var gradeDtos = new List<GradeDto>();
                _ = grades.OrderBy(g => g.GradeName);

                foreach (var grade in grades)
                {
                    gradeDtos.Add(new GradeDto
                    {
                        Id = grade.Id,
                        Name = grade.GradeName,
                        Description = grade.Description ?? string.Empty
                    });
                }

                return ApiResponse<List<GradeDto>>.SuccessResult(gradeDtos);
            }
            catch (Exception)
            {
                return ApiResponse<List<GradeDto>>.ErrorResult("Lỗi hệ thống");
            }
        }

        public async Task<ApiResponse<GradeDto>> GetGradeByIdAsync(Guid id)
        {
            try
            {
                Grade? grade = await _uow.Grades.GetByIdAsync(id);

                if (grade == null)
                {
                    return ApiResponse<GradeDto>.ErrorResult("Không tìm thấy khối học với ID đã chọn");
                }

                GradeDto gradeDto = new()
                {
                    Id = grade!.Id,
                    Name = grade.GradeName,
                    Description = grade.Description ?? string.Empty
                };

                return ApiResponse<GradeDto>.SuccessResult(gradeDto);
            }
            catch (Exception)
            {
                return ApiResponse<GradeDto>.ErrorResult("Lỗi hệ thống");
            }
        }
    }
}
