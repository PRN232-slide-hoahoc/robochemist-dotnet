using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RoboChemist.TemplateService.Model.DTOs;

/// <summary>
/// Request for uploading a generic file
/// </summary>
public class FileUploadRequest
{
    [Required(ErrorMessage = "File is required")]
    public IFormFile File { get; set; } = null!;
}

/// <summary>
/// Response after uploading a generic file
/// </summary>
public class FileUploadResponse
{
    public string ObjectKey { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}
