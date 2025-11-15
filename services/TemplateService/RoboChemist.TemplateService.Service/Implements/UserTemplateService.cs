using RoboChemist.Shared.Common.Constants;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.Shared.DTOs.UserDTOs;
using RoboChemist.TemplateService.Model.DTOs;
using RoboChemist.TemplateService.Model.Models;
using RoboChemist.TemplateService.Repository.Interfaces;
using RoboChemist.TemplateService.Service.HttpClients;
using RoboChemist.TemplateService.Service.Interfaces;

namespace RoboChemist.TemplateService.Service.Implements;

/// <summary>
/// UserTemplate service implementation
/// </summary>
public class UserTemplateService : IUserTemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthServiceClient _authServiceClient;

    public UserTemplateService(
        IUnitOfWork unitOfWork,
        IAuthServiceClient authServiceClient)
    {
        _unitOfWork = unitOfWork;
        _authServiceClient = authServiceClient;
    }

    public async Task<ApiResponse<IEnumerable<UserTemplateResponse>>> GetMyTemplatesAsync()
    {
        // Get current user
        UserDto? user = await _authServiceClient.GetCurrentUserAsync();
        if (user == null)
        {
            return ApiResponse<IEnumerable<UserTemplateResponse>>.ErrorResult("User not authenticated");
        }

        var userTemplates = await _unitOfWork.UserTemplates.GetUserTemplatesByUserIdAsync(user.Id);
        
        var response = userTemplates.Select(ut => new UserTemplateResponse
        {
            TemplateId = ut.Template.TemplateId,
            ObjectKey = ut.Template.ObjectKey,
            TemplateName = ut.Template.TemplateName,
            Description = ut.Template.Description,
            ThumbnailUrl = ut.Template.ThumbnailUrl,
            PreviewUrl = ut.Template.PreviewUrl,
            ContentStructure = ut.Template.ContentStructure,
            SlideCount = ut.Template.SlideCount,
            IsPremium = ut.Template.IsPremium,
            Price = ut.Template.Price,
            DownloadCount = ut.Template.DownloadCount,
            CreatedAt = ut.Template.CreatedAt,
            UpdatedAt = ut.Template.UpdatedAt,
            CreatedBy = ut.Template.CreatedBy,
            Version = ut.Template.Version
        });

        return ApiResponse<IEnumerable<UserTemplateResponse>>.SuccessResult(response, "Retrieved user templates successfully");
    }

    public async Task<ApiResponse<bool>> CheckTemplateAccessAsync(Guid templateId)
    {
        // Get current user
        UserDto? user = await _authServiceClient.GetCurrentUserAsync();
        if (user == null)
        {
            return ApiResponse<bool>.ErrorResult("User not authenticated");
        }

        // Check if user has access
        bool hasAccess = await _unitOfWork.UserTemplates.UserHasTemplateAsync(user.Id, templateId);
        
        return ApiResponse<bool>.SuccessResult(hasAccess, hasAccess ? "User has access" : "User does not have access");
    }

    public async Task<ApiResponse<UserTemplateResponse>> GrantTemplateAccessAsync(GrantTemplateAccessRequest request)
    {
        // Get current user
        UserDto? user = await _authServiceClient.GetCurrentUserAsync();
        if (user == null)
        {
            return ApiResponse<UserTemplateResponse>.ErrorResult("User not authenticated");
        }

        // Validate template exists
        var template = await _unitOfWork.Templates.GetByIdAsync(request.TemplateId);
        if (template == null)
        {
            return ApiResponse<UserTemplateResponse>.ErrorResult("Template not found");
        }

        // Check if user already has this template
        bool alreadyHas = await _unitOfWork.UserTemplates.UserHasTemplateAsync(user.Id, request.TemplateId);
        if (alreadyHas)
        {
            return ApiResponse<UserTemplateResponse>.ErrorResult("User already has access to this template");
        }

        // Create user template
        var userTemplate = new UserTemplate
        {
            UserTemplateId = Guid.NewGuid(),
            UserId = user.Id,
            TemplateId = request.TemplateId
        };

        await _unitOfWork.UserTemplates.CreateAsync(userTemplate);

        var response = new UserTemplateResponse
        {
            TemplateId = template.TemplateId,
            ObjectKey = template.ObjectKey,
            TemplateName = template.TemplateName,
            Description = template.Description,
            ThumbnailUrl = template.ThumbnailUrl,
            PreviewUrl = template.PreviewUrl,
            ContentStructure = template.ContentStructure,
            SlideCount = template.SlideCount,
            IsPremium = template.IsPremium,
            Price = template.Price,
            DownloadCount = template.DownloadCount,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            CreatedBy = template.CreatedBy,
            Version = template.Version
        };

        return ApiResponse<UserTemplateResponse>.SuccessResult(response, "Template access granted successfully");
    }

    public async Task<ApiResponse<bool>> RevokeTemplateAccessAsync(Guid userTemplateId)
    {
        var userTemplate = await _unitOfWork.UserTemplates.GetByIdAsync(userTemplateId);
        if (userTemplate == null)
        {
            return ApiResponse<bool>.ErrorResult("User template not found");
        }

        await _unitOfWork.UserTemplates.RemoveAsync(userTemplate);

        return ApiResponse<bool>.SuccessResult(true, "Template access revoked successfully");
    }

    public async Task<ApiResponse<IEnumerable<UserTemplateResponse>>> GetUserTemplatesByUserIdAsync(Guid userId)
    {
        var userTemplates = await _unitOfWork.UserTemplates.GetUserTemplatesByUserIdAsync(userId);
        
        var response = userTemplates.Select(ut => new UserTemplateResponse
        {
            TemplateId = ut.Template.TemplateId,
            ObjectKey = ut.Template.ObjectKey,
            TemplateName = ut.Template.TemplateName,
            Description = ut.Template.Description,
            ThumbnailUrl = ut.Template.ThumbnailUrl,
            PreviewUrl = ut.Template.PreviewUrl,
            ContentStructure = ut.Template.ContentStructure,
            SlideCount = ut.Template.SlideCount,
            IsPremium = ut.Template.IsPremium,
            Price = ut.Template.Price,
            DownloadCount = ut.Template.DownloadCount,
            CreatedAt = ut.Template.CreatedAt,
            UpdatedAt = ut.Template.UpdatedAt,
            CreatedBy = ut.Template.CreatedBy,
            Version = ut.Template.Version
        });

        return ApiResponse<IEnumerable<UserTemplateResponse>>.SuccessResult(response, "Retrieved user templates successfully");
    }
}
