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
        public async Task<ApiResponse<List<GetGradeDto>>> GetGrades()
        {
            try
            {
                List<Grade> grades = await _unitOfWork.Grades.GetAllAsync();
                var gradeDtos = new List<GetGradeDto>();

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
            catch (Exception ex)
            {
                return ApiResponse<List<GetGradeDto>>.ErrorResult("Lỗi hệ thống");
            }
        }
    }
}
