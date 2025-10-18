using Microsoft.AspNetCore.Mvc;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.SlidesService.Service.Interfaces;
using static RoboChemist.Shared.DTOs.GradeDTOs.GradeDTOs;

namespace RoboChemist.SlidesService.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class GradeController : ControllerBase
    {
        private readonly IGradeService _gradeService;

        public GradeController(IGradeService gradeService)
        {
            _gradeService = gradeService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<GetGradeDto>>>> GetGrades()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<GetGradeDto>.ErrorResult("Dữ liệu xác thực không hợp lệ", errors));
                }

                var result = await _gradeService.GetGrades();

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<GetGradeDto>.ErrorResult("Lỗi hệ thống"));
            }
        }
    }
}
