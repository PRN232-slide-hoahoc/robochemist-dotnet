using RoboChemist.Shared.DTOs.Common;
using RoboChemist.TemplateService.Model.DTOs;

namespace RoboChemist.TemplateService.Service.Interfaces;

/// <summary>
/// UserTemplate service interface for managing user template access
/// </summary>
public interface IUserTemplateService
{
    /// <summary>
    /// Get all templates accessible by current user
    /// </summary>
    Task<ApiResponse<IEnumerable<UserTemplateResponse>>> GetMyTemplatesAsync();

    /// <summary>
    /// Check if current user has access to a specific template
    /// </summary>
    Task<ApiResponse<bool>> CheckTemplateAccessAsync(Guid templateId);

    /// <summary>
    /// Purchase a template (orchestrates payment and access granting)
    /// </summary>
    Task<ApiResponse<PurchaseTemplateResponse>> PurchaseTemplateAsync(PurchaseTemplateRequest request);
}
