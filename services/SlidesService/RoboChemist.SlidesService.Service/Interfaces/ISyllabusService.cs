using RoboChemist.Shared.DTOs.Common;
using static RoboChemist.Shared.DTOs.SyllabusDTOs.SyllabusRequestDTOs;
using static RoboChemist.Shared.DTOs.SyllabusDTOs.SyllabusResponseDTOs;

namespace RoboChemist.SlidesService.Service.Interfaces
{
    public interface ISyllabusService
    {
        Task<ApiResponse<List<SyllabusDto>>> GetSyllabusesAsync(Guid? gradeId, Guid? topicId);

        Task<ApiResponse<SyllabusDto>> GetSyllabusAsync(Guid id);

        Task<ApiResponse<SyllabusDto>> CreateSyllabusAsync(CreateSyllabusRequestDto request);

        Task<ApiResponse<SyllabusDto>> UpdateSyllabusAsync(Guid id, CreateSyllabusRequestDto request);

        Task<ApiResponse<bool>> ToggleSyllabusStatusAsync(Guid id);
    }
}
