using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RoboChemist.TemplateService.Model.DTOs;

#region Template Requests

/// <summary>
/// Request for uploading a new template
/// </summary>
public class UploadTemplateRequest
{
    [Required(ErrorMessage = "File is required")]
    public IFormFile? File { get; set; }

    public IFormFile? ThumbnailFile { get; set; }

    [Required(ErrorMessage = "Template name is required")]
    [MaxLength(255)]
    public string TemplateName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SlideCount { get; set; }

    public bool IsPremium { get; set; }

    public decimal Price { get; set; }
}

/// <summary>
/// Request for updating an existing template
/// </summary>
public class UpdateTemplateRequest
{
    [Required(ErrorMessage = "Template name is required")]
    [MaxLength(255)]
    public string TemplateName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SlideCount { get; set; }

    public bool IsPremium { get; set; }

    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;
}

#endregion

#region Template Responses

/// <summary>
/// Response after uploading a template
/// </summary>
public class UploadTemplateResponse
{
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

#endregion
