using RoboChemist.Shared.DTOs.UserDTOs;
using RoboChemist.TemplateService.Model.DTOs;
using RoboChemist.TemplateService.Model.Models;
using RoboChemist.TemplateService.Repository.Interfaces;
using RoboChemist.TemplateService.Service.HttpClients;
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
    private readonly IAuthServiceClient _authServiceClient;

    #endregion

    #region Constructor

    public TemplateService(
        IUnitOfWork unitOfWork, 
        IStorageService storageService,
        IAuthServiceClient authServiceClient)
    {
        _unitOfWork = unitOfWork;
        _storageService = storageService;
        _authServiceClient = authServiceClient;
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

    public async Task<PagedResult<Template>> GetPagedTemplatesForStaffAsync(PaginationParams paginationParams)
    {
        return await _unitOfWork.Templates.GetPagedTemplatesForStaffAsync(paginationParams);
    }

    #endregion

    #region Command Methods

    public async Task<UploadTemplateResponse> UploadTemplateAsync(Stream fileStream, string fileName, UploadTemplateRequest request)
    {
        // Get user information from AuthService (like SlidesService)
        UserDto? user = await _authServiceClient.GetCurrentUserAsync();
        if (user == null)
        {
            throw new UnauthorizedAccessException("Người dùng không hợp lệ");
        }

        string? objectKey = null;
        string? thumbnailUrl = null;
        
        try
        {
            // Upload template file
            objectKey = await _storageService.UploadFileAsync(fileStream, fileName, "templates");

            // Upload thumbnail if provided
            if (request.ThumbnailFile != null && request.ThumbnailFile.Length > 0)
            {
                using var thumbnailStream = request.ThumbnailFile.OpenReadStream();
                var thumbnailObjectKey = await _storageService.UploadFileAsync(
                    thumbnailStream, 
                    request.ThumbnailFile.FileName, 
                    "thumbnails");
                
                // Generate presigned URL (valid 1 year)
                thumbnailUrl = await _storageService.GeneratePresignedUrlAsync(thumbnailObjectKey, 525600);
            }

            var template = new Template
            {
                TemplateId = Guid.NewGuid(),
                ObjectKey = objectKey,
                TemplateName = request.TemplateName,
                Description = request.Description,
                SlideCount = request.SlideCount,
                IsPremium = request.IsPremium,
                Price = request.Price,
                IsActive = true,
                DownloadCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Version = 1,
                CreatedBy = user.Id,
                ThumbnailUrl = thumbnailUrl
            };

            await _unitOfWork.Templates.CreateAsync(template);

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
            // Cleanup on error
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

    public async Task<Template> UpdateTemplateAsync(Guid templateId, UpdateTemplateRequest request)
    {
        var template = await _unitOfWork.Templates.GetByIdAsync(templateId);
        
        if (template == null)
            throw new KeyNotFoundException($"Template with ID {templateId} not found");

        try
        {
            // Update only editable template properties (không update ObjectKey, CreatedAt, CreatedBy)
            template.TemplateName = request.TemplateName;
            template.Description = request.Description;
            template.SlideCount = request.SlideCount;
            template.IsPremium = request.IsPremium;
            template.Price = request.Price;
            template.IsActive = request.IsActive;
            template.UpdatedAt = DateTime.UtcNow;
            template.Version++;

            // Đảm bảo ObjectKey không bị null (required field)
            if (string.IsNullOrEmpty(template.ObjectKey))
            {
                throw new InvalidOperationException("Template ObjectKey cannot be null or empty");
            }

            await _unitOfWork.Templates.UpdateAsync(template);

            return template;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to update template: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteTemplateAsync(Guid templateId)
    {
        var template = await _unitOfWork.Templates.GetByIdAsync(templateId);
        
        if (template == null)
            return false;

        // Soft delete - chỉ thay đổi IsActive thành false
        template.IsActive = false;
        template.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Templates.UpdateAsync(template);

        return true;
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
        await _unitOfWork.Templates.UpdateAsync(template);

        return await _storageService.DownloadFileAsync(template.ObjectKey);
    }

    public async Task<string> GeneratePresignedUrlAsync(Guid templateId, int expirationMinutes = 60)
    {
        var template = await _unitOfWork.Templates.GetByIdAsync(templateId);
        
        if (template == null)
            throw new KeyNotFoundException($"Template with ID {templateId} not found");

        if (!template.IsActive)
            throw new InvalidOperationException($"Template {templateId} is not active");

        return await _storageService.GeneratePresignedUrlAsync(template.ObjectKey, expirationMinutes);
    }

    #endregion
}
