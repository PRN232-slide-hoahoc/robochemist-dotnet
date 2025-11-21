using RoboChemist.TemplateService.Model.Models;
using RoboChemist.Shared.Common.GenericRepositories;

namespace RoboChemist.TemplateService.Repository.Interfaces;

public interface IUserTemplateRepository : IGenericRepository<UserTemplate>
{
    Task<IEnumerable<UserTemplate>> GetUserTemplatesByUserIdAsync(Guid userId);
    Task<bool> UserHasTemplateAsync(Guid userId, Guid templateId);
}