using Microsoft.AspNetCore.Mvc;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.SlidesService.Service.Interfaces;
using static RoboChemist.Shared.DTOs.GradeDTOs.GradeDTOs;

namespace RoboChemist.SlidesService.API.Controllers
{
    [Route("api/v1/grades")]
    [ApiController]
    public class GradeController : ControllerBase
    {
        private readonly IGradeService _gradeService;

        public GradeController(IGradeService gradeService)
        {
            _gradeService = gradeService;
        }

        /// <summary>
        /// Get all grades in database
        /// </summary>
        /// <returns>List of all Grades in database</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<GradeDto>>>> GetGrades()
        {
            try
            {
                var result = await _gradeService.GetGradesAsync();

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<GradeDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Get grade by Id
        /// </summary>
        /// <param name="id">Id of grade in Guid</param>
        /// <returns>A Grade information of exactly provided id</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<List<GradeDto>>>> GetGradeById([FromRoute] Guid id)
        {
            try
            {
                var result = await _gradeService.GetGradeByIdAsync(id);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<GradeDto>.ErrorResult("Lỗi hệ thống"));
            }
        }
    }
}
