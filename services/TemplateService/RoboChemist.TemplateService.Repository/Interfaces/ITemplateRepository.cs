using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.TemplateService.Model.DTOs;
using RoboChemist.TemplateService.Model.Models;

namespace RoboChemist.TemplateService.Repository.Interfaces;

/// <summary>
/// Template repository interface with specific methods
/// </summary>
public interface ITemplateRepository : IGenericRepository<Template>
{
    /// <summary>
    /// Get active templates
    /// </summary>
    Task<IEnumerable<Template>> GetActiveTemplatesAsync();

    /// <summary>
    /// Get paged templates with filters (only active templates for public users)
    /// </summary>
    Task<PagedResult<Template>> GetPagedTemplatesAsync(PaginationParams paginationParams);

    /// <summary>
    /// Get paged templates with filters including INACTIVE templates (for Staff/Admin management)
    /// </summary>
    Task<PagedResult<Template>> GetPagedTemplatesForStaffAsync(PaginationParams paginationParams);

    /// <summary>
    /// Get premium templates
    /// </summary>
    Task<IEnumerable<Template>> GetPremiumTemplatesAsync();

    /// <summary>
    /// Increment download count
    /// </summary>
    Task IncrementDownloadCountAsync(Guid templateId);
}

