using Microsoft.AspNetCore.Mvc;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.ExamService.Service.Interfaces;
using static RoboChemist.Shared.DTOs.QuestionDTOs.QuestionDTOs;

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
        /// Get all questions, optionally filtered by topic
        /// </summary>
        /// <param name="topicId">Optional: Filter questions by topic ID from Slide Service</param>
        /// <returns>List of questions</returns>
        /// <response code="200">Returns the list of questions</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<QuestionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<QuestionDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<List<QuestionDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<QuestionDto>>>> GetQuestions([FromQuery] Guid? topicId)
        {
            try
            {
                var result = await _questionService.GetQuestionsAsync(topicId);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<QuestionDto>>.ErrorResult($"Lỗi hệ thống: {ex.Message}"));
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
        [ProducesResponseType(typeof(ApiResponse<QuestionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<QuestionDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<QuestionDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<QuestionDto>>> GetQuestionById([FromRoute] Guid id)
        {
            try
            {
                var result = await _questionService.GetQuestionByIdAsync(id);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<QuestionDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}"));
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
        ///         "questionType": "Multiple Choice",
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
        [ProducesResponseType(typeof(ApiResponse<QuestionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<QuestionDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<QuestionDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<QuestionDto>>> CreateQuestion([FromBody] CreateQuestionDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<QuestionDto>.ErrorResult("Dữ liệu xác thực không hợp lệ", errors));
                }

                var result = await _questionService.CreateQuestionAsync(request);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<QuestionDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}"));
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
        [ProducesResponseType(typeof(ApiResponse<QuestionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<QuestionDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<QuestionDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<QuestionDto>>> UpdateQuestion(
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

                    return BadRequest(ApiResponse<QuestionDto>.ErrorResult("Dữ liệu xác thực không hợp lệ", errors));
                }

                var result = await _questionService.UpdateQuestionAsync(id, request);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<QuestionDto>.ErrorResult($"Lỗi hệ thống: {ex.Message}"));
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
    }
}
