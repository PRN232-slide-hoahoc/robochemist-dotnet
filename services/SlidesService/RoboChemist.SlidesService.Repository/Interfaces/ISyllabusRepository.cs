using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.SlidesService.Model.Models;
using static RoboChemist.Shared.DTOs.SyllabusDTOs.SyllabusResponseDTOs;

namespace RoboChemist.SlidesService.Repository.Interfaces
{
    public interface ISyllabusRepository : IGenericRepository<Syllabus>
    {
        Task<List<SyllabusDto>> GetFullInformationAsync(Guid? gradeId, Guid? topicId);
    }
}
