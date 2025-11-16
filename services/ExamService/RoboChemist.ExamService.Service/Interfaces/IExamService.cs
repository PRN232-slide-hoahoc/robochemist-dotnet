using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.ExamServiceDTOs.ExamDTOs;

namespace RoboChemist.ExamService.Service.Interfaces
{
    /// <summary>
    /// Interface cho Exam Service - Quản lý việc tạo và quản lý đề thi
    /// </summary>
    public interface IExamService
    {
        /// <summary>
        /// Tạo yêu cầu tạo đề thi mới và tự động generate câu hỏi. Tự động lấy userId từ JWT token.
        /// User chỉ có thể tạo đề từ ma trận do chính mình tạo.
        /// </summary>
        /// <param name="createExamRequestDto">Thông tin yêu cầu tạo đề (MatrixId, Price)</param>
        /// <returns>Thông tin yêu cầu tạo đề và đề thi đã được generate</returns>
        Task<ApiResponse<ExamRequestResponseDto>> CreateExamRequestAsync(CreateExamRequestDto createExamRequestDto);

        /// <summary>
        /// Lấy thông tin yêu cầu tạo đề theo ID
        /// </summary>
        /// <param name="examRequestId">ID yêu cầu tạo đề</param>
        /// <returns>Thông tin yêu cầu</returns>
        Task<ApiResponse<ExamRequestResponseDto>> GetExamRequestByIdAsync(Guid examRequestId);

        /// <summary>
        /// Lấy danh sách yêu cầu tạo đề của người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="status">Lọc theo trạng thái (null = tất cả)</param>
        /// <returns>Danh sách yêu cầu tạo đề</returns>
        Task<ApiResponse<List<ExamRequestResponseDto>>> GetExamRequestsByUserAsync(Guid userId, string? status = null);

        /// <summary>
        /// Lấy thông tin đề thi đã được tạo theo ID
        /// </summary>
        /// <param name="generatedExamId">ID đề thi</param>
        /// <returns>Thông tin đề thi</returns>
        Task<ApiResponse<GeneratedExamResponseDto>> GetGeneratedExamByIdAsync(Guid generatedExamId);

        /// <summary>
        /// Export đề thi ra file Word (.docx) - Lấy cùng bộ câu hỏi đã generate
        /// </summary>
        /// <param name="generatedExamId">ID đề thi đã được generate</param>
        /// <returns>File Word dạng byte array</returns>
        Task<ApiResponse<byte[]>> ExportExamToWordAsync(Guid generatedExamId);

        /// <summary>
        /// Cập nhật trạng thái đề thi (PENDING -> READY -> EXPIRED)
        /// </summary>
        /// <param name="generatedExamId">ID đề thi</param>
        /// <param name="status">Trạng thái mới</param>
        /// <returns>Thông tin đề thi sau khi cập nhật</returns>
        Task<ApiResponse<GeneratedExamResponseDto>> UpdateExamStatusAsync(Guid generatedExamId, string status);

        /// <summary>
        /// Xóa đề thi đã được tạo
        /// </summary>
        /// <param name="generatedExamId">ID đề thi</param>
        /// <returns>Kết quả xóa</returns>
        Task<ApiResponse<bool>> DeleteGeneratedExamAsync(Guid generatedExamId);
    }
}
