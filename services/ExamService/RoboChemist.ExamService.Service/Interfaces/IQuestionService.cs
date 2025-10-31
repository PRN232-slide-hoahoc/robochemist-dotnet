using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.ExamServiceDTOs.QuestionDTOs;

namespace RoboChemist.ExamService.Service.Interfaces
{
    /// <summary>
    /// Interface cho Question Service - Quản lý câu hỏi
    /// </summary>
    public interface IQuestionService
    {
        /// <summary>
        /// Tạo mới Question với các Options
        /// </summary>
        /// <param name="createQuestionDto">Thông tin câu hỏi cần tạo</param>
        /// <returns>Thông tin câu hỏi đã tạo</returns>
        /// <remarks>UserId và AuthToken được lấy tự động từ HttpContext</remarks>
        Task<ApiResponse<QuestionResponseDto>> CreateQuestionAsync(CreateQuestionDto createQuestionDto);

        /// <summary>
        /// Lấy thông tin Question theo ID
        /// </summary>
        /// <param name="id">ID của Question</param>
        /// <returns>Thông tin chi tiết Question</returns>
        Task<ApiResponse<QuestionResponseDto>> GetQuestionByIdAsync(Guid id);

        /// <summary>
        /// Lấy danh sách Questions, có thể lọc theo TopicId và Status
        /// </summary>
        /// <param name="topicId">Optional: Lọc theo TopicId (Guid)</param>
        /// <param name="status">Optional: Lọc theo Status ("0" hoặc "1")</param>
        /// <returns>Danh sách Questions</returns>
        Task<ApiResponse<List<QuestionResponseDto>>> GetQuestionsAsync(Guid? topicId = null, string? status = null);

        /// <summary>
        /// Cập nhật Question
        /// </summary>
        /// <param name="id">ID của Question cần cập nhật</param>
        /// <param name="updateQuestionDto">Thông tin cập nhật</param>
        /// <returns>Thông tin Question sau khi cập nhật</returns>
        /// <remarks>UserId và AuthToken được lấy tự động từ HttpContext</remarks>
        Task<ApiResponse<QuestionResponseDto>> UpdateQuestionAsync(Guid id, UpdateQuestionDto updateQuestionDto);

        /// <summary>
        /// Xóa Question (soft delete - set Status = "0")
        /// </summary>
        /// <param name="id">ID của Question cần xóa</param>
        /// <returns>Kết quả xóa</returns>
        /// <remarks>UserId được lấy tự động từ HttpContext</remarks>
        Task<ApiResponse<bool>> DeleteQuestionAsync(Guid id);

        /// <summary>
        /// [TEMP - FOR SEEDING] Tạo nhiều Questions cùng 1 Topic
        /// </summary>
        /// <param name="bulkCreateDto">Thông tin bulk create</param>
        /// <returns>Thông tin các câu hỏi đã tạo</returns>
        Task<ApiResponse<BulkCreateQuestionsResponseDto>> BulkCreateQuestionsAsync(BulkCreateQuestionsDto bulkCreateDto);
    }
}
