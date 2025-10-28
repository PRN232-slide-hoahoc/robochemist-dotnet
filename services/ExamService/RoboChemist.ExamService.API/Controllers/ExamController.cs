using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RoboChemist.Shared.Common.Helpers;
using RoboChemist.ExamService.Service.Interfaces;
using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.ExamServiceDTOs.ExamDTOs;

namespace RoboChemist.ExamService.API.Controllers
{
    /// <summary>
    /// Controller quản lý Đề thi (GeneratedExam)
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize] // Yêu cầu authentication cho tất cả endpoints
    public class ExamController : ControllerBase
    {
        private readonly IExamService _examService;

        public ExamController(IExamService examService)
        {
            _examService = examService;
        }

        /// <summary>
        /// Tạo yêu cầu tạo đề thi mới
        /// </summary>
        /// <param name="createExamRequestDto">Thông tin yêu cầu tạo đề</param>
        /// <returns>Thông tin yêu cầu đã tạo</returns>
        [HttpPost("request")]
        public async Task<ActionResult<ApiResponse<ExamRequestResponseDto>>> CreateExamRequest([FromBody] CreateExamRequestDto createExamRequestDto)
        {
            // Validate ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<ExamRequestResponseDto>.ErrorResult(string.Join("; ", errors)));
            }

            // Lấy UserId từ JWT token bằng JwtHelper
            if (!JwtHelper.TryGetUserId(User, out var userId))
            {
                return Unauthorized(ApiResponse<ExamRequestResponseDto>.ErrorResult("Token không hợp lệ hoặc không có userId"));
            }

            var result = await _examService.CreateExamRequestAsync(createExamRequestDto, userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetExamRequestById), new { id = result.Data!.ExamRequestId }, result);
        }

        /// <summary>
        /// Lấy thông tin yêu cầu tạo đề theo ID
        /// </summary>
        /// <param name="id">ID yêu cầu tạo đề</param>
        /// <returns>Thông tin yêu cầu</returns>
        [HttpGet("request/{id}")]
        public async Task<ActionResult<ApiResponse<ExamRequestResponseDto>>> GetExamRequestById(Guid id)
        {
            var result = await _examService.GetExamRequestByIdAsync(id);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách yêu cầu tạo đề của người dùng
        /// </summary>
        /// <param name="userId">ID người dùng (tạm thời từ query, sau này từ token)</param>
        /// <param name="status">Lọc theo trạng thái (Pending/Completed/Failed)</param>
        /// <returns>Danh sách yêu cầu tạo đề</returns>
        [HttpGet("request/user/{userId}")]
        public async Task<ActionResult<ApiResponse<List<ExamRequestResponseDto>>>> GetExamRequestsByUser(Guid userId, [FromQuery] string? status = null)
        {
            var result = await _examService.GetExamRequestsByUserAsync(userId, status);
            return Ok(result);
        }

        /// <summary>
        /// Xử lý tạo đề thi từ yêu cầu (Generate exam)
        /// </summary>
        /// <param name="examRequestId">ID yêu cầu tạo đề</param>
        /// <returns>Thông tin đề thi đã được tạo</returns>
        [HttpPost("generate/{examRequestId}")]
        public async Task<ActionResult<ApiResponse<GeneratedExamResponseDto>>> GenerateExam(Guid examRequestId)
        {
            var result = await _examService.GenerateExamAsync(examRequestId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin đề thi đã được tạo theo ID
        /// </summary>
        /// <param name="id">ID đề thi</param>
        /// <returns>Thông tin đề thi chi tiết</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<GeneratedExamResponseDto>>> GetGeneratedExamById(Guid id)
        {
            var result = await _examService.GetGeneratedExamByIdAsync(id);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Cập nhật trạng thái đề thi
        /// </summary>
        /// <param name="id">ID đề thi</param>
        /// <param name="status">Trạng thái mới (Draft/Published/Archived)</param>
        /// <returns>Thông tin đề thi sau khi cập nhật</returns>
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<ApiResponse<GeneratedExamResponseDto>>> UpdateExamStatus(Guid id, [FromBody] string status)
        {
            var result = await _examService.UpdateExamStatusAsync(id, status);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Xóa đề thi đã được tạo
        /// </summary>
        /// <param name="id">ID đề thi</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteGeneratedExam(Guid id)
        {
            var result = await _examService.DeleteGeneratedExamAsync(id);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}
