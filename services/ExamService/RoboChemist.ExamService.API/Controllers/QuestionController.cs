using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.ExamService.Service.Interfaces;
using static RoboChemist.Shared.DTOs.ExamServiceDTOs.QuestionDTOs;

namespace RoboChemist.ExamService.API.Controllers
{
    /// <summary>
    /// Controller for managing exam questions
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
        /// Get all questions, optionally filtered by topic and search term
        /// </summary>
        /// <param name="topicId">Optional: Filter questions by topic ID from Slide Service</param>
        /// <param name="search">Optional: Search term for question text</param>
        /// <returns>List of questions</returns>
        /// <response code="200">Returns the list of questions</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpGet]
        [AllowAnonymous] // Ai cũng có thể xem danh sách câu hỏi
        [ProducesResponseType(typeof(ApiResponse<List<QuestionResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<QuestionResponseDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<List<QuestionResponseDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<QuestionResponseDto>>>> GetQuestions(
            [FromQuery] Guid? topicId,
            [FromQuery] string? search)
        {
            try
            {
                var result = await _questionService.GetQuestionsAsync(topicId, search);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<QuestionResponseDto>>.ErrorResult($"Lỗi hệ thống: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get a specific question by ID
        /// </summary>
        /// <param name="id">The unique identifier of the question</param>
        /// <returns>Question details with all options</returns>
        /// <response code="200">Returns the requested question</response>
        /// <response code="400">If the question is not found or request is invalid</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpGet("{id}")]
        [AllowAnonymous] // Ai cũng có thể xem chi tiết câu hỏi
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<QuestionResponseDto>>> GetQuestionById([FromRoute] Guid id)
        {
            try
            {
                var result = await _questionService.GetQuestionByIdAsync(id);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<QuestionResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create a new question with answer options
        /// </summary>
        /// <param name="request">Question creation details including options</param>
        /// <returns>The newly created question</returns>
        /// <response code="200">Returns the newly created question</response>
        /// <response code="400">If the request data is invalid (validation errors, topic not found, etc.)</response>
        /// <response code="500">If there is an internal server error</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/v1/Question
        ///     {
        ///         "topicId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///         "questionType": "MultipleChoice",
        ///         "questionText": "Công thức hóa học của nước là gì?",
        ///         "explanation": "Nước được tạo thành từ 2 nguyên tử Hydro và 1 nguyên tử Oxygen",
        ///         "options": [
        ///             {
        ///                 "answer": "H2O",
        ///                 "isCorrect": true
        ///             },
        ///             {
        ///                 "answer": "CO2",
        ///                 "isCorrect": false
        ///             },
        ///             {
        ///                 "answer": "O2",
        ///                 "isCorrect": false
        ///             },
        ///             {
        ///                 "answer": "H2",
        ///                 "isCorrect": false
        ///             }
        ///         ]
        ///     }
        /// 
        /// </remarks>
        [HttpPost]
        [Authorize(Roles = "Teacher,Admin")] // Chỉ Teacher và Admin mới được tạo câu hỏi
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<QuestionResponseDto>>> CreateQuestion([FromBody] CreateQuestionDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<QuestionResponseDto>.ErrorResult("Dữ liệu xác thực không hợp lệ", errors));
                }

                var result = await _questionService.CreateQuestionAsync(request);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<QuestionResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update an existing question
        /// </summary>
        /// <param name="id">The unique identifier of the question to update</param>
        /// <param name="request">Updated question details</param>
        /// <returns>The updated question</returns>
        /// <response code="200">Returns the updated question</response>
        /// <response code="400">If the request data is invalid or question not found</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Teacher,Admin")] // Chỉ Teacher và Admin mới được sửa câu hỏi
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<QuestionResponseDto>>> UpdateQuestion(
            [FromRoute] Guid id, 
            [FromBody] UpdateQuestionDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<QuestionResponseDto>.ErrorResult("Dữ liệu xác thực không hợp lệ", errors));
                }

                var result = await _questionService.UpdateQuestionAsync(id, request);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<QuestionResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete a question (soft delete - sets IsActive to false)
        /// </summary>
        /// <param name="id">The unique identifier of the question to delete</param>
        /// <returns>Success status</returns>
        /// <response code="200">Returns success if question was deleted</response>
        /// <response code="400">If the question is not found</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Teacher,Admin")] // Chỉ Teacher và Admin mới được xóa câu hỏi
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteQuestion([FromRoute] Guid id)
        {
            try
            {
                var result = await _questionService.DeleteQuestionAsync(id);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResult($"Lỗi hệ thống: {ex.Message}"));
            }
        }

        /// <summary>
        /// Bulk create multiple questions for a single topic
        /// </summary>
        /// <param name="request">Bulk creation details with topic ID and list of questions</param>
        /// <returns>Bulk creation result with created question IDs</returns>
        /// <response code="200">Returns the bulk creation result</response>
        /// <response code="400">If the request data is invalid or topic not found</response>
        /// <response code="500">If there is an internal server error</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/v1/Question/bulk
        ///     {
        ///         "topicId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///         "questions": [
        ///             {
        ///                 "questionType": "MultipleChoice",
        ///                 "questionText": "Câu hỏi 1?",
        ///                 "explanation": "Giải thích câu 1",
        ///                 "options": [
        ///                     { "answer": "Đáp án A", "isCorrect": true },
        ///                     { "answer": "Đáp án B", "isCorrect": false }
        ///                 ]
        ///             },
        ///             {
        ///                 "questionType": "TrueFalse",
        ///                 "questionText": "Câu hỏi 2?",
        ///                 "explanation": "Giải thích câu 2",
        ///                 "options": [
        ///                     { "answer": "Đúng", "isCorrect": true },
        ///                     { "answer": "Sai", "isCorrect": false }
        ///                 ]
        ///             }
        ///         ]
        ///     }
        /// 
        /// </remarks>
        [HttpPost("bulk")]
        [Authorize(Roles = "Teacher,Admin")] // Chỉ Teacher và Admin mới được bulk create
        [ProducesResponseType(typeof(ApiResponse<BulkCreateQuestionsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<BulkCreateQuestionsResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<BulkCreateQuestionsResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<BulkCreateQuestionsResponseDto>>> BulkCreateQuestions([FromBody] BulkCreateQuestionsDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<BulkCreateQuestionsResponseDto>.ErrorResult("Dữ liệu xác thực không hợp lệ", errors));
                }

                var result = await _questionService.BulkCreateQuestionsAsync(request);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<BulkCreateQuestionsResponseDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}"));
            }
        }
    }
}
