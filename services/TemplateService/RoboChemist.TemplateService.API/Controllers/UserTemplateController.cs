using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.TemplateService.Model.DTOs;
using RoboChemist.TemplateService.Service.Interfaces;

namespace RoboChemist.TemplateService.API.Controllers;

/// <summary>
/// User Template management endpoints
/// </summary>
[Route("api/v1/user-templates")]
[ApiController]
public class UserTemplateController : ControllerBase
{
    private readonly IUserTemplateService _userTemplateService;

    public UserTemplateController(IUserTemplateService userTemplateService)
    {
        _userTemplateService = userTemplateService;
    }

    /// <summary>
    /// Get all templates accessible by current user
    /// </summary>
    /// <returns>List of user templates with access details</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Staff,User")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserTemplateResponse>>>> GetMyTemplates()
    {
        var result = await _userTemplateService.GetMyTemplatesAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Check if current user has access to a specific template
    /// </summary>
    /// <param name="templateId">Template ID to check</param>
    /// <returns>True if user has access, false otherwise</returns>
    [HttpGet("{templateId}/access")]
    [Authorize(Roles = "Admin,Staff,User")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckTemplateAccess(Guid templateId)
    {
        var result = await _userTemplateService.CheckTemplateAccessAsync(templateId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Grant template access to current user (used after purchase or subscription)
    /// </summary>
    /// <param name="request">Grant access request with template ID and access type</param>
    /// <returns>Created user template details</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Staff,User")]
    public async Task<ActionResult<ApiResponse<UserTemplateResponse>>> GrantTemplateAccess([FromBody] GrantTemplateAccessRequest request)
    {
        var result = await _userTemplateService.GrantTemplateAccessAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Increment usage count for a template (called when user uses the template)
    /// </summary>
    /// <param name="templateId">Template ID to increment usage</param>
    /// <returns>Updated user template details</returns>
    [HttpPost("{templateId}/usage")]
    [Authorize(Roles = "Admin,Staff,User")]
    public async Task<ActionResult<ApiResponse<UserTemplateResponse>>> IncrementTemplateUsage(Guid templateId)
    {
        var result = await _userTemplateService.IncrementTemplateUsageAsync(templateId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Revoke template access (Admin only)
    /// </summary>
    /// <param name="userTemplateId">User template ID to revoke</param>
    /// <Presignedreturns>Success status</returns>
    [HttpDelete("{userTemplateId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> RevokeTemplateAccess(Guid userTemplateId)
    {
        var result = await _userTemplateService.RevokeTemplateAccessAsync(userTemplateId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get user templates by user ID (Admin/Staff only)
    /// </summary>
    /// <param name="userId">User ID to retrieve templates for</param>
    /// <returns>List of user templates</returns>
    [HttpGet("users/{userId}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserTemplateResponse>>>> GetUserTemplatesByUserId(Guid userId)
    {
        var result = await _userTemplateService.GetUserTemplatesByUserIdAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
