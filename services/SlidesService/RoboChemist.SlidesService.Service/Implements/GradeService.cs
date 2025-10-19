using RoboChemist.Shared.DTOs.Common;
using RoboChemist.SlidesService.Model.Models;
using RoboChemist.SlidesService.Repository.Interfaces;
using RoboChemist.SlidesService.Service.Interfaces;
using static RoboChemist.Shared.DTOs.GradeDTOs.GradeDTOs;

namespace RoboChemist.SlidesService.Service.Implements
{
    public class GradeService : IGradeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GradeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<List<GetGradeDto>>> GetGradesAsync()
        {
            try
            {
                List<Grade> grades = await _unitOfWork.Grades.GetAllAsync();
                var gradeDtos = new List<GetGradeDto>();
                _ = grades.OrderBy(g => g.GradeName);

                foreach (var grade in grades)
                {
                    gradeDtos.Add(new GetGradeDto
                    {
                        Id = grade.Id,
                        Name = grade.GradeName,
                        Description = grade.Description ?? string.Empty
                    });
                }

                return ApiResponse<List<GetGradeDto>>.SuccessResul(gradeDtos);
            }
            catch (Exception)
            {
                return ApiResponse<List<GetGradeDto>>.ErrorResult("Lỗi hệ thống");
            }
        }

        public async Task<ApiResponse<GetGradeDto>> GetGradeByIdAsync(Guid id)
        {
            try
            {
                Grade? grade = await _unitOfWork.Grades.GetByIdAsync(id);

                if (grade == null)
                {
                    return ApiResponse<GetGradeDto>.ErrorResult("Không tìm thấy khối học với ID đã chọn");
                }

                GetGradeDto gradeDto = new()
                {
                    Id = grade!.Id,
                    Name = grade.GradeName,
                    Description = grade.Description ?? string.Empty
                };

                return ApiResponse<GetGradeDto>.SuccessResul(gradeDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<GetGradeDto>.ErrorResult("Lỗi hệ thống");
            }
        }
    }
}
