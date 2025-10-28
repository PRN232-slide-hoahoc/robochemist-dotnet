namespace RoboChemist.ExamService.Service.HttpClients
{
    /// <summary>
    /// Interface để gọi các API của SlidesService
    /// </summary>
    public interface ISlidesServiceHttpClient
    {
        /// <summary>
        /// Lấy thông tin Topic theo ID (GET /api/v1/Topic/{id})
        /// </summary>
        /// <param name="topicId">ID của Topic cần kiểm tra</param>
        /// <param name="authToken">JWT token để authorize</param>
        /// <returns>True nếu Topic tồn tại, False nếu không tồn tại hoặc lỗi</returns>
        Task<bool> GetTopicByIdAsync(Guid topicId, string? authToken = null);

        /// <summary>
        /// Lấy thông tin chi tiết Topic (trả về DTO nếu tồn tại, null nếu không tồn tại hoặc lỗi)
        /// </summary>
        Task<RoboChemist.Shared.DTOs.TopicDTOs.TopicDTOs.TopicDto?> GetTopicAsync(Guid topicId, string? authToken = null);
    }
}
