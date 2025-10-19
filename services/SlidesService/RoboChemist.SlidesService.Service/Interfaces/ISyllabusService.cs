using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.SyllabusDTOs.SyllabusRequestDTOs;
using static RoboChemist.Shared.DTOs.SyllabusDTOs.SyllabusResponseDTOs;

namespace RoboChemist.SlidesService.Service.Interfaces
{
    /// <summary>
    /// Service interface for managing syllabus operations
    /// </summary>
    public interface ISyllabusService
    {
        /// <summary>
        /// Gets syllabuses with optional filtering by grade and/or topic
        /// </summary>
        /// <param name="gradeId">Optional grade ID to filter syllabuses</param>
        /// <param name="topicId">Optional topic ID to filter syllabuses</param>
        /// <returns>ApiResponse containing list of syllabus DTOs or error message</returns>
        Task<ApiResponse<List<SyllabusDto>>> GetSyllabusesAsync(Guid? gradeId, Guid? topicId);

        /// <summary>
        /// Gets a specific syllabus by its ID
        /// </summary>
        /// <param name="id">The unique identifier of the syllabus</param>
        /// <returns>ApiResponse containing the syllabus DTO or error message</returns>
        Task<ApiResponse<SyllabusDto>> GetSyllabusAsync(Guid id);

        /// <summary>
        /// Creates a new syllabus in the system
        /// </summary>
        /// <param name="request">Data for creating the syllabus</param>
        /// <returns>ApiResponse containing the created syllabus DTO or error message</returns>
        Task<ApiResponse<SyllabusDto>> CreateSyllabusAsync(CreateSyllabusRequestDto request);

        /// <summary>
        /// Updates an existing syllabus with new information
        /// </summary>
        /// <param name="id">The unique identifier of the syllabus to update</param>
        /// <param name="request">Updated syllabus data</param>
        /// <returns>ApiResponse containing the updated syllabus DTO or error message</returns>
        Task<ApiResponse<SyllabusDto>> UpdateSyllabusAsync(Guid id, CreateSyllabusRequestDto request);

        /// <summary>
        /// Toggles the active status of a syllabus (activates if inactive, deactivates if active)
        /// </summary>
        /// <param name="id">The unique identifier of the syllabus</param>
        /// <returns>ApiResponse containing the new active status or error message</returns>
        Task<ApiResponse<bool>> ToggleSyllabusStatusAsync(Guid id);
    }
}
