using Microsoft.AspNetCore.Mvc;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.SlidesService.Service.Interfaces;
using static RoboChemist.Shared.DTOs.TopicDTOs.TopicDTOs;

namespace RoboChemist.SlidesService.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class TopicController : ControllerBase
    {
        private readonly ITopicService _topicService;

        public TopicController(ITopicService topicService)
        {
            _topicService = topicService;
        }

        /// <summary>
        /// Get all topics, optionally filtered by grade
        /// </summary>
        /// <param name="gradeId">Optional: Filter topics by grade ID</param>
        /// <returns>List of topics</returns>
        /// <response code="200">Returns the list of topics</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<TopicDto>>>> GetTopics([FromQuery] Guid? gradeId)
        {
            try
            {
                var result = await _topicService.GetTopicsAsync(gradeId);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<TopicDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Create a new topic
        /// </summary>
        /// <param name="request">Topic creation details</param>
        /// <returns>The newly created topic</returns>
        /// <response code="200">Returns the newly created topic</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<TopicDto>>> CreateTopic([FromBody]CreateTopicDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<TopicDto>.ErrorResult("Dữ liệu xác thực không hợp lệ", errors));
                }

                var result = await _topicService.CreateTopicAsync(request);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<TopicDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Get a specific topic by ID
        /// </summary>
        /// <param name="id">The unique identifier of the topic</param>
        /// <returns>Topic details</returns>
        /// <response code="200">Returns the requested topic</response>
        /// <response code="400">If the topic is not found or request is invalid</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TopicDto>>> GetTopicById([FromRoute] Guid id)
        {
            try
            {
                var result = await _topicService.GetTopicByIdAsync(id);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<TopicDto>.ErrorResult("Lỗi hệ thống"));
            }
        }
    }
}
