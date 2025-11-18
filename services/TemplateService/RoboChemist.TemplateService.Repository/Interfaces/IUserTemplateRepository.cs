using RoboChemist.TemplateService.Model.Models;
using RoboChemist.Shared.Common.GenericRepositories;

namespace RoboChemist.TemplateService.Repository.Interfaces;

/// <summary>
/// UserTemplate repository interface
/// </summary>
public interface IUserTemplateRepository : IGenericRepository<UserTemplate>
{
    /// <summary>
    /// Get user templates by user ID
    /// </summary>
    Task<IEnumerable<UserTemplate>> GetUserTemplatesByUserIdAsync(Guid userId);

    /// <summary>
    /// Check if user has template
    /// </summary>
    Task<bool> UserHasTemplateAsync(Guid userId, Guid templateId);
}

