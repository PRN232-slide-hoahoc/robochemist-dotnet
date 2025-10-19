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

        [HttpPost]
        public async Task<ActionResult<ApiResponse<List<SyllabusDto>>>> CreateSyllabus([FromBody] CreateSyllabusRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<List<SyllabusDto>>.ErrorResult("Dữ liệu xác thực không hợp lệ", errors));
                }

                var result = await _syllabusService.CreateSyllabusAsync(request);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<List<SyllabusDto>>.ErrorResult("Lỗi hệ thống"));
            }
        }

        [HttpPut]
        public async Task<ActionResult<ApiResponse<List<SyllabusDto>>>> UpdateSyllabus([FromRoute] Guid id, [FromBody] CreateSyllabusRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<List<SyllabusDto>>.ErrorResult("Dữ liệu xác thực không hợp lệ", errors));
                }

                var result = await _syllabusService.UpdateSyllabusAsync(id, request);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<List<SyllabusDto>>.ErrorResult("Lỗi hệ thống"));
            }
        }

        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult<ApiResponse<List<SyllabusDto>>>> ToggleSyllabusStatus([FromRoute] Guid id)
        {
            try
            {
                var result = await _syllabusService.ToggleSyllabusStatusAsync(id);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<List<SyllabusDto>>.ErrorResult("Lỗi hệ thống"));
            }
        }
    }
}
