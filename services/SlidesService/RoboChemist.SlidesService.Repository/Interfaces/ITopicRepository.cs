using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.SlidesService.Model.Models;
using static RoboChemist.Shared.DTOs.TopicDTOs.TopicDTOs;

namespace RoboChemist.SlidesService.Repository.Interfaces
{
    public interface ITopicRepository : IGenericRepository<Topic>
    {
        /// <summary>
        /// provides full topic details including grade name
        /// </summary>
        Task<List<TopicDto>> GetFullTopicsAsync(Guid? gradeId);
    }
}
