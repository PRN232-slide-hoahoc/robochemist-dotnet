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
            UserTemplateId = ut.UserTemplateId,
            UserId = ut.UserId,
            TemplateId = ut.TemplateId,
            TemplateName = ut.Template.TemplateName,
            TemplateType = ut.Template.TemplateType,
            AccessType = ut.AccessType,
            AcquiredAt = ut.AcquiredAt,
            ExpiresAt = ut.ExpiresAt,
            UsageCount = ut.UsageCount,
            UsageLimit = ut.UsageLimit,
            IsActive = ut.IsActive
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
            TemplateId = request.TemplateId,
            AccessType = request.AccessType,
            AcquiredAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt,
            UsageLimit = request.UsageLimit,
            UsageCount = 0,
            IsActive = true
        };

        await _unitOfWork.UserTemplates.CreateAsync(userTemplate);

        var response = new UserTemplateResponse
        {
            UserTemplateId = userTemplate.UserTemplateId,
            UserId = userTemplate.UserId,
            TemplateId = userTemplate.TemplateId,
            TemplateName = template.TemplateName,
            TemplateType = template.TemplateType,
            AccessType = userTemplate.AccessType,
            AcquiredAt = userTemplate.AcquiredAt,
            ExpiresAt = userTemplate.ExpiresAt,
            UsageCount = userTemplate.UsageCount,
            UsageLimit = userTemplate.UsageLimit,
            IsActive = userTemplate.IsActive
        };

        return ApiResponse<UserTemplateResponse>.SuccessResult(response, "Template access granted successfully");
    }

    public async Task<ApiResponse<UserTemplateResponse>> IncrementTemplateUsageAsync(Guid templateId)
    {
        // Get current user
        UserDto? user = await _authServiceClient.GetCurrentUserAsync();
        if (user == null)
        {
            return ApiResponse<UserTemplateResponse>.ErrorResult("User not authenticated");
        }

        // Find user template
        var userTemplates = await _unitOfWork.UserTemplates.GetUserTemplatesByUserIdAsync(user.Id);
        var userTemplate = userTemplates.FirstOrDefault(ut => ut.TemplateId == templateId && ut.IsActive);

        if (userTemplate == null)
        {
            return ApiResponse<UserTemplateResponse>.ErrorResult("User does not have access to this template");
        }

        // Check if expired
        if (userTemplate.ExpiresAt.HasValue && userTemplate.ExpiresAt.Value < DateTime.UtcNow)
        {
            return ApiResponse<UserTemplateResponse>.ErrorResult("Template access has expired");
        }

        // Check if usage limit reached
        if (userTemplate.UsageLimit.HasValue && userTemplate.UsageCount >= userTemplate.UsageLimit.Value)
        {
            return ApiResponse<UserTemplateResponse>.ErrorResult("Template usage limit reached");
        }

        // Increment usage count
        userTemplate.UsageCount++;
        await _unitOfWork.UserTemplates.UpdateAsync(userTemplate);

        var response = new UserTemplateResponse
        {
            UserTemplateId = userTemplate.UserTemplateId,
            UserId = userTemplate.UserId,
            TemplateId = userTemplate.TemplateId,
            TemplateName = userTemplate.Template.TemplateName,
            TemplateType = userTemplate.Template.TemplateType,
            AccessType = userTemplate.AccessType,
            AcquiredAt = userTemplate.AcquiredAt,
            ExpiresAt = userTemplate.ExpiresAt,
            UsageCount = userTemplate.UsageCount,
            UsageLimit = userTemplate.UsageLimit,
            IsActive = userTemplate.IsActive
        };

        return ApiResponse<UserTemplateResponse>.SuccessResult(response, "Template usage incremented successfully");
    }

    public async Task<ApiResponse<bool>> RevokeTemplateAccessAsync(Guid userTemplateId)
    {
        var userTemplate = await _unitOfWork.UserTemplates.GetByIdAsync(userTemplateId);
        if (userTemplate == null)
        {
            return ApiResponse<bool>.ErrorResult("User template not found");
        }

        userTemplate.IsActive = false;
        await _unitOfWork.UserTemplates.UpdateAsync(userTemplate);

        return ApiResponse<bool>.SuccessResult(true, "Template access revoked successfully");
    }

    public async Task<ApiResponse<IEnumerable<UserTemplateResponse>>> GetUserTemplatesByUserIdAsync(Guid userId)
    {
        var userTemplates = await _unitOfWork.UserTemplates.GetUserTemplatesByUserIdAsync(userId);
        
        var response = userTemplates.Select(ut => new UserTemplateResponse
        {
            UserTemplateId = ut.UserTemplateId,
            UserId = ut.UserId,
            TemplateId = ut.TemplateId,
            TemplateName = ut.Template.TemplateName,
            TemplateType = ut.Template.TemplateType,
            AccessType = ut.AccessType,
            AcquiredAt = ut.AcquiredAt,
            ExpiresAt = ut.ExpiresAt,
            UsageCount = ut.UsageCount,
            UsageLimit = ut.UsageLimit,
            IsActive = ut.IsActive
        });

        return ApiResponse<IEnumerable<UserTemplateResponse>>.SuccessResult(response, "Retrieved user templates successfully");
    }
}
