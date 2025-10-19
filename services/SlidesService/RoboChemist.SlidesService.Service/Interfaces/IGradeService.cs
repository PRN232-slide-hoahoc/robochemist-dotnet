using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.GradeDTOs.GradeDTOs;

namespace RoboChemist.SlidesService.Service.Interfaces
{
    public interface IGradeService
    {
        /// <summary>
        /// Retrieves a list of grades.
        /// </summary>
        /// <returns>An <see cref="ApiResponse{T}"/> containing a list of <see cref="GetGradeDto"/> objects representing the
        /// grades. The response includes metadata such as success status and error messages, if any.</returns>
        Task<ApiResponse<List<GetGradeDto>>> GetGradesAsync();

        /// <summary>
        /// Gets grade by id.
        /// </summary>
        /// <param name="id">Guid value of Grade id</param>
        /// <returns><see cref="GetGradeDto"/> object representing the grade with the specified id, or an error response if not found.</returns>
        Task<ApiResponse<GetGradeDto>> GetGradeByIdAsync(Guid id);
    }
}
