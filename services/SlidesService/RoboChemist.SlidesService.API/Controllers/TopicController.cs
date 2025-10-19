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

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<GetTopicDto>>>> GetTopics([FromQuery] Guid? gradeId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<GetTopicDto>.ErrorResult("Dữ liệu xác thực không hợp lệ", errors));
                }

                var result = await _topicService.GetTopicsAsync(gradeId);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<GetTopicDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<GetTopicDto>>> CreateTopic([FromBody]CreateTopicDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<GetTopicDto>.ErrorResult("Dữ liệu xác thực không hợp lệ", errors));
                }

                var result = await _topicService.CreateTopicAsync(request);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<GetTopicDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        [HttpGet("/{id}")]
        public async Task<ActionResult<ApiResponse<GetTopicDto>>> GetTopicById([FromRoute] Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<GetTopicDto>.ErrorResult("Dữ liệu xác thực không hợp lệ", errors));
                }

                var result = await _topicService.GetTopicByIdAsync(id);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<GetTopicDto>.ErrorResult("Lỗi hệ thống"));
            }
        }
    }
}
