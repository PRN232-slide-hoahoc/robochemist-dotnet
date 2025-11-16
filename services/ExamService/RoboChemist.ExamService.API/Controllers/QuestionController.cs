using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.ExamService.Service.Interfaces;
using static RoboChemist.Shared.DTOs.ExamServiceDTOs.QuestionDTOs;
using RoboChemist.Shared.Common.Constants;

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
        [HttpGet]
        [AllowAnonymous] 
        public async Task<ActionResult<ApiResponse<List<QuestionResponseDto>>>> GetQuestions(
            [FromQuery] Guid? topicId,
            [FromQuery] string? search)
        {
            try
            {
                var result = await _questionService.GetQuestionsAsync(topicId, search);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<List<QuestionResponseDto>>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Get a specific question by ID
        /// </summary>
        /// <param name="id">The unique identifier of the question</param>
        /// <returns>Question details with all options</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<QuestionResponseDto>>> GetQuestionById([FromRoute] Guid id)
        {
            try
            {
                var result = await _questionService.GetQuestionByIdAsync(id);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<QuestionResponseDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Create a new question with answer options
        /// </summary>
        /// <param name="request">Question creation details including options</param>
        /// <returns>The newly created question</returns>
        [HttpPost]
        [Authorize(Roles = RoboChemistConstants.ROLE_ADMIN)] 
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
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<QuestionResponseDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Update an existing question
        /// </summary>
        /// <param name="id">The unique identifier of the question to update</param>
        /// <param name="request">Updated question details</param>
        /// <returns>The updated question</returns>
      
        [HttpPut("{id}")]
        [Authorize(Roles = RoboChemistConstants.ROLE_ADMIN)] 
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
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<QuestionResponseDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Delete a question (soft delete - sets IsActive to false)
        /// </summary>
        /// <param name="id">The unique identifier of the question to delete</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = RoboChemistConstants.ROLE_ADMIN)] 
        public async Task<ActionResult<ApiResponse<bool>>> DeleteQuestion([FromRoute] Guid id)
        {
            try
            {
                var result = await _questionService.DeleteQuestionAsync(id);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Bulk create multiple questions for a single topic
        /// </summary>
        /// <param name="request">Bulk creation details with topic ID and list of questions</param>
        /// <returns>Bulk creation result with created question IDs</returns>
        [HttpPost("bulk")]
        [Authorize(Roles = RoboChemistConstants.ROLE_ADMIN)]
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
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<BulkCreateQuestionsResponseDto>.ErrorResult("Lỗi hệ thống"));
            }
        }
    }
}
