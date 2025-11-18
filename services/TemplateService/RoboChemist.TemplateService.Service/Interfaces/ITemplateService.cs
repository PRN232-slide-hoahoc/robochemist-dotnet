using RoboChemist.TemplateService.Model.DTOs;
using RoboChemist.TemplateService.Model.Models;

namespace RoboChemist.TemplateService.Service.Interfaces;

public interface ITemplateService
{
    Task<UploadTemplateResponse> UploadTemplateAsync(Stream fileStream, string fileName, UploadTemplateRequest request);
    Task<Template?> GetTemplateByIdAsync(Guid templateId);
    Task<IEnumerable<Template>> GetAllTemplatesAsync();
    
    /// <summary>
    /// Get paged templates - Returns ONLY ACTIVE templates for public users
    /// </summary>
    Task<PagedResult<Template>> GetPagedTemplatesAsync(PaginationParams paginationParams);
    
    /// <summary>
    /// Get paged templates for Staff/Admin - Returns ALL templates (including inactive)
    /// </summary>
    Task<PagedResult<Template>> GetPagedTemplatesForStaffAsync(PaginationParams paginationParams);
    
    Task<Template> UpdateTemplateAsync(Guid templateId, UpdateTemplateRequest request);
    Task<bool> DeleteTemplateAsync(Guid templateId);
    Task<(Stream FileStream, string ContentType, string FileName)> DownloadTemplateAsync(Guid templateId);
    Task<string> GeneratePresignedUrlAsync(Guid templateId, int expirationMinutes = 60);
}

