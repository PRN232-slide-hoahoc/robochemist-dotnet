using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RoboChemist.ExamService.Service.Interfaces;
using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.ExamServiceDTOs.ExamDTOs;

namespace RoboChemist.ExamService.API.Controllers
{
    /// <summary>
    /// Controller quản lý Đề thi (GeneratedExam)
    /// </summary>
    [Route("api/v1/exams")]
    [ApiController]
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
        [Authorize(Roles = "Teacher,Admin")] // Chỉ Teacher và Admin mới được tạo yêu cầu đề thi
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

            // Lấy UserId từ JWT token
            var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            
            // Debug: Print all claims
            Console.WriteLine("[EXAM CONTROLLER] JWT Claims:");
            foreach (var claim in User?.Claims ?? Enumerable.Empty<System.Security.Claims.Claim>())
            {
                Console.WriteLine($"  - {claim.Type}: {claim.Value}");
            }
            
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                Console.WriteLine($"[EXAM CONTROLLER ERROR] UserId claim not found or invalid. NameIdentifier claim: {userIdClaim?.Value ?? "NULL"}");
                return Unauthorized(ApiResponse<ExamRequestResponseDto>.ErrorResult(
                    "Token không hợp lệ hoặc không có userId"));
            }

            Console.WriteLine($"[EXAM CONTROLLER] Extracted UserId from JWT: {userId}");

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
        [Authorize] // Phải đăng nhập mới xem được exam request
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
        [Authorize] // Phải đăng nhập mới xem được exam requests của user
        public async Task<ActionResult<ApiResponse<List<ExamRequestResponseDto>>>> GetExamRequestsByUser(Guid userId, [FromQuery] string? status = null)
        {
            var result = await _examService.GetExamRequestsByUserAsync(userId, status);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin đề thi đã được tạo theo ID
        /// </summary>
        /// <param name="id">ID đề thi</param>
        /// <returns>Thông tin đề thi chi tiết</returns>
        [HttpGet("{id}")]
        [AllowAnonymous] // Ai cũng có thể xem đề thi đã generate
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
        /// Export đề thi ra file Word (.docx) - Lấy cùng bộ câu hỏi đã generate
        /// </summary>
        /// <param name="generatedExamId">ID đề thi đã được generate</param>
        /// <returns>File Word (.docx)</returns>
        [HttpGet("{generatedExamId}/export/word")]
        [AllowAnonymous] // Ai cũng có thể export đề thi ra Word
        public async Task<IActionResult> ExportExamToWord(Guid generatedExamId)
        {
            var result = await _examService.ExportExamToWordAsync(generatedExamId);

            if (!result.Success || result.Data == null)
            {
                return BadRequest(result);
            }

            var fileName = $"DeThi_{generatedExamId}_{DateTime.Now:yyyyMMddHHmmss}.docx";
            
            return File(
                result.Data,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                fileName
            );
        }

        /// <summary>
        /// Cập nhật trạng thái đề thi
        /// </summary>
        /// <param name="id">ID đề thi</param>
        /// <param name="status">Trạng thái mới (Draft/Published/Archived)</param>
        /// <returns>Thông tin đề thi sau khi cập nhật</returns>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Teacher,Admin")] // Chỉ Teacher và Admin mới được cập nhật trạng thái đề thi
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
        [Authorize(Roles = "Teacher,Admin")] // Chỉ Teacher và Admin mới được xóa đề thi
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
