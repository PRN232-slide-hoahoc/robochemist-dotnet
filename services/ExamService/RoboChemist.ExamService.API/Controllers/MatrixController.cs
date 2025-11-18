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
    [Route("api/v1/matrices")]
    [ApiController]
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
            try
            {
                var result = await _matrixService.GetMatrixByIdAsync(id);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<MatrixResponseDto>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả ma trận đề thi
        /// </summary>
        /// <param name="isActive">Lọc theo trạng thái (true/false/null = tất cả)</param>
        /// <returns>Danh sách ma trận</returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<MatrixResponseDto>>>> GetAllMatrices([FromQuery] bool? isActive = null)
        {
            try
            {
                var result = await _matrixService.GetAllMatricesAsync(isActive);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<List<MatrixResponseDto>>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Lấy danh sách thông tin cơ bản của tất cả ma trận (không bao gồm chi tiết)
        /// </summary>
        /// <param name="isActive">Lọc theo trạng thái: true=active, false=inactive, null=tất cả</param>
        /// <returns>Danh sách thông tin cơ bản của ma trận</returns>
        [HttpGet("names")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<MatrixBasicDto>>>> GetAllMatrixNames([FromQuery] bool? isActive = null)
        {
            try
            {
                var result = await _matrixService.GetAllMatrixNamesAsync(isActive);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<List<MatrixBasicDto>>.ErrorResult("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Tạo mới một ma trận đề thi kèm chi tiết phân bổ câu hỏi theo chủ đề.
        /// Tự động validate số lượng câu hỏi có sẵn trong hệ thống.
        /// </summary>
        /// <param name="createDto">Thông tin ma trận cần tạo</param>
        /// <returns>Ma trận vừa tạo</returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<MatrixResponseDto>>> CreateMatrix([FromBody] CreateMatrixDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<MatrixResponseDto>.ErrorResult("Dữ liệu xác thực không hợp lệ", errors));
                }

                var result = await _matrixService.CreateMatrixAsync(createDto);

                return result.Success 
                    ? CreatedAtAction(nameof(GetMatrixById), new { id = result.Data!.MatrixId }, result)
                    : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<MatrixResponseDto>.ErrorResult("Lỗi hệ thống"));
            }
        }
    }
}
