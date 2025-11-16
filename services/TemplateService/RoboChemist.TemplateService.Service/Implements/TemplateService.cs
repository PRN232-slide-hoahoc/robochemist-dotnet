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
        var pagedResult = await _unitOfWork.Templates.GetPagedTemplatesAsync(paginationParams);
        
        // Generate presigned URLs for thumbnails (7 days expiration)
        await GenerateThumbnailPresignedUrlsAsync(pagedResult.Items);
        
        return pagedResult;
    }

    public async Task<PagedResult<Template>> GetPagedTemplatesForStaffAsync(PaginationParams paginationParams)
    {
        var pagedResult = await _unitOfWork.Templates.GetPagedTemplatesForStaffAsync(paginationParams);
        
        // Generate presigned URLs for thumbnails (7 days expiration)
        await GenerateThumbnailPresignedUrlsAsync(pagedResult.Items);
        
        return pagedResult;
    }
    
    /// <summary>
    /// Generate presigned URLs for template thumbnails from object keys
    /// </summary>
    private async Task GenerateThumbnailPresignedUrlsAsync(IEnumerable<Template> templates)
    {
        foreach (var template in templates)
        {
            if (!string.IsNullOrEmpty(template.ThumbnailUrl))
            {
                // Extract object key from old presigned URL or use as-is if already object key
                string objectKey;
                
                if (template.ThumbnailUrl.StartsWith("http"))
                {
                    // Old presigned URL format, extract object key
                    // Example: https://...r2.cloudflarestorage.com/bucket-name/thumbnails/file.png?...
                    var uri = new Uri(template.ThumbnailUrl);
                    objectKey = uri.AbsolutePath.TrimStart('/');
                    
                    // Remove bucket name from path if present
                    if (objectKey.StartsWith("template-service-bucket/"))
                    {
                        objectKey = objectKey.Replace("template-service-bucket/", "");
                    }
                }
                else
                {
                    // Already an object key
                    objectKey = template.ThumbnailUrl;
                }
                
                // Generate new presigned URL (7 days = 10,080 minutes)
                try
                {
                    template.ThumbnailUrl = await _storageService.GeneratePresignedUrlAsync(objectKey, 10080);
                }
                catch (Exception ex)
                {
                    // If generation fails, set to null and log
                    Console.WriteLine($"Failed to generate presigned URL for {template.TemplateId}: {ex.Message}");
                    template.ThumbnailUrl = null;
                }
            }
        }
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
                
                // Store only object key, will generate presigned URL when retrieving
                thumbnailUrl = thumbnailObjectKey;
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
