using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.ExamServiceDTOs.MatrixDTOs;

namespace RoboChemist.ExamService.Service.Interfaces
{
    /// <summary>
    /// Interface cho Matrix Service - Quản lý ma trận đề thi (Chỉ đọc)
    /// </summary>
    public interface IMatrixService
    {
        /// <summary>
        /// Lấy thông tin ma trận theo ID
        /// </summary>
        /// <param name="id">ID của ma trận</param>
        /// <returns>Thông tin ma trận</returns>
        Task<ApiResponse<MatrixResponseDto>> GetMatrixByIdAsync(Guid id);

        /// <summary>
        /// Lấy danh sách tất cả ma trận đề thi
        /// </summary>
        /// <param name="isActive">Lọc theo trạng thái (null = tất cả)</param>
        /// <returns>Danh sách ma trận</returns>
        Task<ApiResponse<List<MatrixResponseDto>>> GetAllMatricesAsync(bool? isActive = null);

        /// <summary>
        /// Tạo mới một ma trận đề thi kèm chi tiết. Tự động lấy userId từ JWT token.
        /// </summary>
        /// <param name="createDto">Thông tin ma trận cần tạo</param>
        /// <returns>Ma trận vừa tạo</returns>
        Task<ApiResponse<MatrixResponseDto>> CreateMatrixAsync(CreateMatrixDto createDto);
    }
}
