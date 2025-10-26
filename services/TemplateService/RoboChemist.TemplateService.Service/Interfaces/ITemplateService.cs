using RoboChemist.TemplateService.Model.DTOs;
using RoboChemist.TemplateService.Model.Models;

namespace RoboChemist.TemplateService.Service.Interfaces;

public interface ITemplateService
{
    Task<UploadTemplateResponse> UploadTemplateAsync(Stream fileStream, string fileName, UploadTemplateRequest request);
    Task<Template?> GetTemplateByIdAsync(Guid templateId);
    Task<IEnumerable<Template>> GetAllTemplatesAsync();
    Task<PagedResult<Template>> GetPagedTemplatesAsync(PaginationParams paginationParams);
    Task<bool> DeleteTemplateAsync(Guid templateId);
    Task<(Stream FileStream, string ContentType, string FileName)> DownloadTemplateAsync(Guid templateId);
}

