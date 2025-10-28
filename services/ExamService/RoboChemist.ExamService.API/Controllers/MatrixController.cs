using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RoboChemist.ExamService.Service.Interfaces;
using RoboChemist.Shared.DTOs.Common;
using System.Security.Claims;
using static RoboChemist.Shared.DTOs.ExamServiceDTOs.MatrixDTOs;

namespace RoboChemist.ExamService.API.Controllers
{
    /// <summary>
    /// Controller quản lý ma trận đề thi
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize] // Yêu cầu authentication cho tất cả endpoints
    public class MatrixController : ControllerBase
    {
        private readonly IMatrixService _matrixService;

        public MatrixController(IMatrixService matrixService)
        {
            _matrixService = matrixService;
        }

        /// <summary>
        /// Lấy thông tin ma trận theo ID
        /// </summary>
        /// <param name="id">ID của ma trận</param>
        /// <returns>Thông tin ma trận</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<MatrixResponseDto>>> GetMatrixById(Guid id)
        {
            var result = await _matrixService.GetMatrixByIdAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách tất cả ma trận đề thi
        /// </summary>
        /// <param name="isActive">Lọc theo trạng thái (true/false/null = tất cả)</param>
        /// <returns>Danh sách ma trận</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<MatrixResponseDto>>>> GetAllMatrices([FromQuery] bool? isActive = null)
        {
            var result = await _matrixService.GetAllMatricesAsync(isActive);
            return Ok(result);
        }

        /// <summary>
        /// Tạo mới một ma trận đề thi kèm chi tiết phân bổ câu hỏi theo chủ đề
        /// </summary>
        /// <param name="createDto">Thông tin ma trận cần tạo</param>
        /// <returns>Ma trận vừa tạo</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<MatrixResponseDto>>> CreateMatrix([FromBody] CreateMatrixDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<MatrixResponseDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
            }

            // Lấy UserId từ JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? createdBy = null;
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
            {
                createdBy = userId;
            }

            var result = await _matrixService.CreateMatrixAsync(createDto, createdBy);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetMatrixById), new { id = result.Data!.MatrixId }, result);
        }
    }
}
