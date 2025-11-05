using Microsoft.AspNetCore.Mvc;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.Shared.DTOs.UserDTOs;
using RoboChemist.SlidesService.Service.HttpClients;
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
        private readonly IAuthServiceClient _auth;
        public SlideController(ISlideService slideService, IAuthServiceClient auth)
        {
            _slideService = slideService;
            _auth = auth;
        }

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

        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> aaa()
        {
            return await _auth.GetCurrentUserAsync();
        }
    }
}
