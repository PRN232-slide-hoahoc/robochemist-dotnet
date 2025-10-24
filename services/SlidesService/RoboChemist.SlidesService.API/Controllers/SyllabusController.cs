using Microsoft.AspNetCore.Mvc;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.SlidesService.Service.Interfaces;
using static RoboChemist.Shared.DTOs.SyllabusDTOs.SyllabusRequestDTOs;
using static RoboChemist.Shared.DTOs.SyllabusDTOs.SyllabusResponseDTOs;

namespace RoboChemist.SlidesService.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SyllabusController : ControllerBase
    {
        private readonly ISyllabusService _syllabusService;
        
        public SyllabusController(ISyllabusService syllabusService)
        {
            _syllabusService = syllabusService;
        }

        /// <summary>
        /// Get all syllabuses with optional filtering by grade and/or topic
        /// </summary>
        /// <param name="gradeId">Optional grade ID to filter syllabuses</param>
        /// <param name="topicId">Optional topic ID to filter syllabuses</param>
        /// <returns>List of syllabuses matching the filter criteria</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<SyllabusDto>>>> GetSyllabuses([FromQuery] Guid? gradeId, Guid? topicId)
        {
            try
            {
                var result = await _syllabusService.GetSyllabusesAsync(gradeId, topicId);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<List<SyllabusDto>>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Get a specific syllabus by ID
        /// </summary>
        /// <param name="id">The unique identifier of the syllabus in Guid format</param>
        /// <returns>Syllabus information for the provided ID</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SyllabusDto>>> GetSyllabus([FromRoute] Guid id)
        {
            try
            {
                var result = await _syllabusService.GetSyllabusAsync(id);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<SyllabusDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Create a new syllabus
        /// </summary>
        /// <param name="request">Syllabus data including topic ID, lesson order, lesson name, objectives, outline, and key concepts</param>
        /// <returns>The newly created syllabus</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<SyllabusDto>>> CreateSyllabus([FromBody] CreateSyllabusRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<SyllabusDto>.ErrorResult("Dữ liệu xác thực không hợp lệ", errors));
                }

                var result = await _syllabusService.CreateSyllabusAsync(request);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<SyllabusDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Update an existing syllabus
        /// </summary>
        /// <param name="id">The unique identifier of the syllabus to update</param>
        /// <param name="request">Updated syllabus data</param>
        /// <returns>The updated syllabus information</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<SyllabusDto>>> UpdateSyllabus([FromRoute] Guid id, [FromBody] CreateSyllabusRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<SyllabusDto>.ErrorResult("Dữ liệu xác thực không hợp lệ", errors));
                }

                var result = await _syllabusService.UpdateSyllabusAsync(id, request);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<SyllabusDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Toggle the active status of a syllabus (activate/deactivate)
        /// </summary>
        /// <param name="id">The unique identifier of the syllabus</param>
        /// <returns>The new active status of the syllabus</returns>
        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult<ApiResponse<bool>>> ToggleSyllabusStatus([FromRoute] Guid id)
        {
            try
            {
                var result = await _syllabusService.ToggleSyllabusStatusAsync(id);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Lỗi hệ thống"));
            }
        }
    }
}
