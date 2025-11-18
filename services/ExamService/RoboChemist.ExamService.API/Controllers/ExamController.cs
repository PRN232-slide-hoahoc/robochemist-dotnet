using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RoboChemist.ExamService.Service.Interfaces;
using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.ExamServiceDTOs.ExamDTOs;
using RoboChemist.Shared.Common.Constants;

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
        /// Tạo yêu cầu tạo đề thi mới. User chỉ có thể tạo đề từ ma trận do chính mình tạo.
        /// </summary>
        /// <param name="createExamRequestDto">Thông tin yêu cầu tạo đề (MatrixId, Price)</param>
        /// <returns>Thông tin yêu cầu tạo đề và đề thi đã được generate</returns>
        [HttpPost("request")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ExamRequestResponseDto>>> CreateExamRequest([FromBody] CreateExamRequestDto createExamRequestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<ExamRequestResponseDto>.ErrorResult("Dữ liệu xác thực không hợp lệ", errors));
                }

                var result = await _examService.CreateExamRequestAsync(createExamRequestDto);

                return result.Success 
                    ? CreatedAtAction(nameof(GetExamRequestById), new { id = result.Data!.ExamRequestId }, result)
                    : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<ExamRequestResponseDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Lấy thông tin yêu cầu tạo đề theo ID
        /// </summary>
        /// <param name="id">ID yêu cầu tạo đề</param>
        /// <returns>Thông tin yêu cầu</returns>
        [HttpGet("request/{id}")]
        [Authorize] 
        public async Task<ActionResult<ApiResponse<ExamRequestResponseDto>>> GetExamRequestById(Guid id)
        {
            try
            {
                var result = await _examService.GetExamRequestByIdAsync(id);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<ExamRequestResponseDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Lấy danh sách yêu cầu tạo đề của người dùng
        /// </summary>
        /// <param name="userId">ID người dùng (tạm thời từ query, sau này từ token)</param>
        /// <param name="status">Lọc theo trạng thái (Pending/Completed/Failed)</param>
        /// <returns>Danh sách yêu cầu tạo đề</returns>
        [HttpGet("request/user/{userId}")]
        [Authorize] 
        public async Task<ActionResult<ApiResponse<List<ExamRequestResponseDto>>>> GetExamRequestsByUser(Guid userId, [FromQuery] string? status = null)
        {
            try
            {
                var result = await _examService.GetExamRequestsByUserAsync(userId, status);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<List<ExamRequestResponseDto>>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Lấy thông tin đề thi đã được tạo theo ID
        /// </summary>
        /// <param name="id">ID đề thi</param>
        /// <returns>Thông tin đề thi chi tiết</returns>
        [HttpGet("{id}")]
        [AllowAnonymous] 
        public async Task<ActionResult<ApiResponse<GeneratedExamResponseDto>>> GetGeneratedExamById(Guid id)
        {
            try
            {
                var result = await _examService.GetGeneratedExamByIdAsync(id);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<GeneratedExamResponseDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Export đề thi ra file Word (.docx) - Lấy cùng bộ câu hỏi đã generate
        /// </summary>
        /// <param name="generatedExamId">ID đề thi đã được generate</param>
        /// <returns>File Word (.docx)</returns>
        [HttpGet("{generatedExamId}/export/word")]
        [AllowAnonymous] 
        public async Task<IActionResult> ExportExamToWord(Guid generatedExamId)
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Lỗi hệ thống khi xuất file", [ex.Message]));
            }
        }

        /// <summary>
        /// Export chỉ đề thi (không có đáp án) ra file Word. Tên file là tên ma trận.
        /// </summary>
        [HttpGet("{generatedExamId}/export/questions")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExamQuestions(Guid generatedExamId)
        {
            try
            {
                var result = await _examService.ExportExamQuestionsOnlyAsync(generatedExamId);

                if (!result.Success || result.Data == null)
                {
                    return BadRequest(result);
                }

                return File(
                    result.Data,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    result.Message ?? "DeThi.docx"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Lỗi hệ thống khi xuất đề thi", [ex.Message]));
            }
        }

        /// <summary>
        /// Export chỉ đáp án ra file Word. Tên file là "DapAn_" + tên ma trận.
        /// </summary>
        [HttpGet("{generatedExamId}/export/answers")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportAnswerKey(Guid generatedExamId)
        {
            try
            {
                var result = await _examService.ExportAnswerKeyOnlyAsync(generatedExamId);

                if (!result.Success || result.Data == null)
                {
                    return BadRequest(result);
                }

                return File(
                    result.Data,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    result.Message ?? "DapAn.docx"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Lỗi hệ thống khi xuất đáp án", [ex.Message]));
            }
        }

        /// <summary>
        /// Cập nhật trạng thái đề thi
        /// </summary>
        /// <param name="id">ID đề thi</param>
        /// <param name="status">Trạng thái mới (Draft/Published/Archived)</param>
        /// <returns>Thông tin đề thi sau khi cập nhật</returns>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = RoboChemistConstants.ROLE_ADMIN)] 
        public async Task<ActionResult<ApiResponse<GeneratedExamResponseDto>>> UpdateExamStatus(Guid id, [FromBody] string status)
        {
            try
            {
                var result = await _examService.UpdateExamStatusAsync(id, status);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<GeneratedExamResponseDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Xóa đề thi đã được tạo
        /// </summary>
        /// <param name="id">ID đề thi</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = RoboChemistConstants.ROLE_ADMIN)] 
        public async Task<ActionResult<ApiResponse<bool>>> DeleteGeneratedExam(Guid id)
        {
            try
            {
                var result = await _examService.DeleteGeneratedExamAsync(id);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Lỗi hệ thống"));
            }
        }
    }
}
