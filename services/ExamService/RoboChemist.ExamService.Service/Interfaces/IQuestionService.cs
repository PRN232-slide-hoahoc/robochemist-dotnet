using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.QuestionDTOs.QuestionDTOs;

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
        Task<ApiResponse<QuestionDto>> CreateQuestionAsync(CreateQuestionDto createQuestionDto);

        /// <summary>
        /// Get a question by ID
        /// </summary>
        /// <param name="id">Question unique identifier</param>
        /// <returns>Question details</returns>
        Task<ApiResponse<QuestionDto>> GetQuestionByIdAsync(Guid id);

        /// <summary>
        /// Get all questions, optionally filtered by topic
        /// </summary>
        /// <param name="topicId">Optional: Filter by topic ID</param>
        /// <returns>List of questions</returns>
        Task<ApiResponse<List<QuestionDto>>> GetQuestionsAsync(Guid? topicId = null);

        /// <summary>
        /// Update an existing question
        /// </summary>
        /// <param name="id">Question ID to update</param>
        /// <param name="updateQuestionDto">Updated question details</param>
        /// <returns>Updated question details</returns>
        Task<ApiResponse<QuestionDto>> UpdateQuestionAsync(Guid id, UpdateQuestionDto updateQuestionDto);

        /// <summary>
        /// Delete a question (soft delete by setting IsActive = false)
        /// </summary>
        /// <param name="id">Question ID to delete</param>
        /// <returns>Success status</returns>
        Task<ApiResponse<bool>> DeleteQuestionAsync(Guid id);
    }
}
