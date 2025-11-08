using Microsoft.AspNetCore.Mvc;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.SlidesService.Service.Interfaces;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideRequestDTOs;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideResponseDTOs;
using static RoboChemist.Shared.DTOs.TopicDTOs.TopicDTOs;

namespace RoboChemist.SlidesService.API.Controllers
{
    [Route("api/v1/slides")]
    [ApiController]
    public class SlideController : ControllerBase
    {
        private readonly ISlideService _slideService;
        public SlideController(ISlideService slideService)
        {
            _slideService = slideService;
        }

        /// <summary>
        /// Generate PowerPoint slides automatically using AI (Gemini). Process flow: authenticate user → payment → AI generates JSON content → apply to PowerPoint template → upload file to storage
        /// </summary>
        /// <param name="request">Slide generation request containing AiPrompt, NumberOfSlides, SyllabusId, and TemplateId</param>
        /// <returns>ApiResponse with SlideDto containing GeneratedSlideId, FilePath, FileSize, SlideCount, GenerationStatus, ProcessingTime, and GeneratedAt</returns>
        [HttpPost("generate")]
        public async Task<ActionResult<ApiResponse<SlideDto>>> GenerateSlide([FromBody] GenerateSlideRequest request)
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

                var result = await _slideService.GenerateSlideAsync(request);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<TopicDto>.ErrorResult("Lỗi hệ thống"));
            }
        }
    }
}
