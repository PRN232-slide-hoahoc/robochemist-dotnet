using RoboChemist.TemplateService.Model.DTOs;
using RoboChemist.TemplateService.Model.Models;
using RoboChemist.TemplateService.Repository.Interfaces;
using RoboChemist.TemplateService.Service.Interfaces;

namespace RoboChemist.TemplateService.Service.Implements;

/// <summary>
/// Implementation of template business logic
/// </summary>
public class TemplateService : ITemplateService
{
    #region Fields

    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;

    #endregion

    #region Constructor

    public TemplateService(IUnitOfWork unitOfWork, IStorageService storageService)
    {
        _unitOfWork = unitOfWork;
        _storageService = storageService;
    }

    #endregion

    #region Query Methods

    public async Task<Template?> GetTemplateByIdAsync(Guid templateId)
    {
        return await _unitOfWork.Templates.GetByIdAsync(templateId);
    }

    public async Task<IEnumerable<Template>> GetAllTemplatesAsync()
    {
        return await _unitOfWork.Templates.GetActiveTemplatesAsync();
    }

    public async Task<PagedResult<Template>> GetPagedTemplatesAsync(PaginationParams paginationParams)
    {
        return await _unitOfWork.Templates.GetPagedTemplatesAsync(paginationParams);
    }

    #endregion

    #region Command Methods

    public async Task<UploadTemplateResponse> UploadTemplateAsync(Stream fileStream, string fileName, UploadTemplateRequest request)
    {
        string? objectKey = null;
        
        try
        {
            objectKey = await _storageService.UploadFileAsync(fileStream, fileName, "templates");
            await _unitOfWork.BeginTransactionAsync();

            var template = new Template
            {
                TemplateId = Guid.NewGuid(),
                ObjectKey = objectKey,
                TemplateName = request.TemplateName,
                TemplateType = request.TemplateType,
                Description = request.Description,
                SlideCount = request.SlideCount,
                IsPremium = request.IsPremium,
                Price = request.Price,
                IsActive = true,
                DownloadCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Version = 1
            };

            await _unitOfWork.Templates.AddAsync(template);
            await _unitOfWork.CommitTransactionAsync();

            return new UploadTemplateResponse
            {
                TemplateId = template.TemplateId,
                TemplateName = template.TemplateName,
                ObjectKey = objectKey,
                Message = "Template uploaded successfully",
                UploadedAt = template.CreatedAt
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            
            if (!string.IsNullOrEmpty(objectKey))
            {
                try
                {
                    await _storageService.DeleteFileAsync(objectKey);
                }
                catch (Exception deleteEx)
                {
                    Console.WriteLine($"Failed to delete orphaned file {objectKey}: {deleteEx.Message}");
                }
            }
            
            throw;
        }
    }

    public async Task<bool> DeleteTemplateAsync(Guid templateId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var template = await _unitOfWork.Templates.GetByIdAsync(templateId);
            if (template == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }

            var objectKey = template.ObjectKey;

            _unitOfWork.Templates.Delete(template);
            await _unitOfWork.CommitTransactionAsync();

            try
            {
                await _storageService.DeleteFileAsync(objectKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to delete file {objectKey} from storage: {ex.Message}");
            }

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<(Stream FileStream, string ContentType, string FileName)> DownloadTemplateAsync(Guid templateId)
    {
        var template = await _unitOfWork.Templates.GetByIdAsync(templateId);
        
        if (template == null)
            throw new KeyNotFoundException($"Template with ID {templateId} not found");

        if (!template.IsActive)
            throw new InvalidOperationException($"Template {templateId} is not active");

        template.DownloadCount++;
        template.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Templates.Update(template);
        await _unitOfWork.SaveChangesAsync();

        return await _storageService.DownloadFileAsync(template.ObjectKey);
    }

    #endregion
}
