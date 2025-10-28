using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.Shared.Common.Helpers;
using RoboChemist.ExamService.Service.Interfaces;
using static RoboChemist.Shared.DTOs.ExamServiceDTOs.QuestionDTOs;

namespace RoboChemist.ExamService.API.Controllers
{
    /// <summary>
    /// Controller quản lý Question - Câu hỏi thi
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController] 
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        /// <summary>
        /// Tạo mới Question với các Options
        /// </summary>
        /// <param name="createQuestionDto">Thông tin câu hỏi cần tạo</param>
        /// <returns>Thông tin câu hỏi đã tạo</returns>
        /// <remarks>
        /// Quy tắc validation:
        /// - TopicId phải tồn tại và active trong SlidesService
        /// - Phải có ít nhất 1 đáp án đúng (trừ Essay)
        /// - TrueFalse: Đúng 2 đáp án
        /// - MultipleChoice: 2-6 đáp án
        /// - Essay: Không cần đáp án
        /// </remarks>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<QuestionResponseDto>>> CreateQuestion([FromBody] CreateQuestionDto createQuestionDto)
        {
            try
            {
                // VALIDATION: ModelState (DTO annotations)
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<QuestionResponseDto>.ErrorResult(string.Join("; ", errors)));
                }

                // Service tự lấy userId và token từ HttpContext
                var result = await _questionService.CreateQuestionAsync(createQuestionDto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                // Return 201 Created with Location header
                return CreatedAtAction(
                    nameof(GetQuestionById),
                    new { id = result.Data!.QuestionId },
                    result
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<QuestionResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy thông tin Question theo ID
        /// </summary>
        /// <param name="id">ID của Question</param>
        /// <returns>Thông tin chi tiết Question</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<QuestionResponseDto>>> GetQuestionById(Guid id)
        {
            try
            {
                var result = await _questionService.GetQuestionByIdAsync(id);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<QuestionResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy danh sách Questions với filter
        /// </summary>
        /// <param name="topicId">Optional: Lọc theo TopicId</param>
        /// <param name="status">Optional: Lọc theo Status ("0" hoặc "1")</param>
        /// <returns>Danh sách Questions</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<QuestionResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<QuestionResponseDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<List<QuestionResponseDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<QuestionResponseDto>>>> GetQuestions(
            [FromQuery] Guid? topicId,  
            [FromQuery] string? status)
        {
            try
            {
                var result = await _questionService.GetQuestionsAsync(topicId, status);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<QuestionResponseDto>>.ErrorResult($"Lỗi hệ thống: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cập nhật Question
        /// </summary>
        /// <param name="id">ID của Question cần cập nhật</param>
        /// <param name="updateQuestionDto">Thông tin cập nhật</param>
        /// <returns>Thông tin Question sau khi cập nhật</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<QuestionResponseDto>>> UpdateQuestion(
            Guid id,
            [FromBody] UpdateQuestionDto updateQuestionDto)
        {
            try
            {
                // VALIDATION: ModelState
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<QuestionResponseDto>.ErrorResult(string.Join("; ", errors)));
                }

                // Service tự lấy userId và token từ HttpContext
                var result = await _questionService.UpdateQuestionAsync(id, updateQuestionDto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<QuestionResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}"));
            }
        }

        /// <summary>
        /// Xóa Question (soft delete - set Status = "0")
        /// </summary>
        /// <param name="id">ID của Question cần xóa</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteQuestion(Guid id)
        {
            try
            {
                // Service tự lấy userId từ HttpContext
                var result = await _questionService.DeleteQuestionAsync(id);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResult($"Lỗi hệ thống: {ex.Message}"));
            }
        }
    }
}
