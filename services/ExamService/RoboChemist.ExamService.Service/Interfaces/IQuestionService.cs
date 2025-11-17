using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.ExamServiceDTOs.QuestionDTOs;

namespace RoboChemist.ExamService.Service.Interfaces
{
    /// <summary>
    /// Interface for Question service operations
    /// </summary>
    public interface IQuestionService
    {
        /// <summary>
        /// Create a new question with options
        /// </summary>
        /// <param name="createQuestionDto">Question creation details</param>
        /// <returns>Created question details</returns>
        Task<ApiResponse<QuestionResponseDto>> CreateQuestionAsync(CreateQuestionDto createQuestionDto);

        /// <summary>
        /// Get a question by ID
        /// </summary>
        /// <param name="id">Question unique identifier</param>
        /// <returns>Question details</returns>
        Task<ApiResponse<QuestionResponseDto>> GetQuestionByIdAsync(Guid id);

        /// <summary>
        /// Get all questions, optionally filtered by topic, search term and level
        /// </summary>
        /// <param name="topicId">Optional: Filter by topic ID</param>
        /// <param name="search">Optional: Search term for question text</param>
        /// <param name="level">Optional: Filter by difficulty level</param>
        /// <returns>List of questions</returns>
        Task<ApiResponse<List<QuestionResponseDto>>> GetQuestionsAsync(Guid? topicId = null, string? search = null, string? level = null);

        /// <summary>
        /// Update an existing question
        /// </summary>
        /// <param name="id">Question ID to update</param>
        /// <param name="updateQuestionDto">Updated question details</param>
        /// <returns>Updated question details</returns>
        Task<ApiResponse<QuestionResponseDto>> UpdateQuestionAsync(Guid id, UpdateQuestionDto updateQuestionDto);

        /// <summary>
        /// Delete a question (soft delete by setting IsActive = false)
        /// </summary>
        /// <param name="id">Question ID to delete</param>
        /// <returns>Success status</returns>
        Task<ApiResponse<bool>> DeleteQuestionAsync(Guid id);

        /// <summary>
        /// Bulk create questions for a topic
        /// </summary>
        /// <param name="bulkCreateDto">Bulk creation details</param>
        /// <returns>Bulk creation result</returns>
        Task<ApiResponse<BulkCreateQuestionsResponseDto>> BulkCreateQuestionsAsync(BulkCreateQuestionsDto request);

        /// <summary>
        /// Đếm số câu hỏi available theo TopicId, QuestionType, và Level
        /// </summary>
        Task<ApiResponse<QuestionCountResponseDto>> CountQuestionsByFiltersAsync(Guid topicId, string questionType, string? level = null);
    }
}
