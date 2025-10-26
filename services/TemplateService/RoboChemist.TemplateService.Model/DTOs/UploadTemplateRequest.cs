using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RoboChemist.TemplateService.Model.DTOs;

public class UploadTemplateRequest
{
    [Required(ErrorMessage = "File is required")]
    public IFormFile? File { get; set; }

    [Required(ErrorMessage = "Template name is required")]
    [MaxLength(255)]
    public string TemplateName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Template type is required")]
    [MaxLength(50)]
    public string TemplateType { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SlideCount { get; set; }

    public bool IsPremium { get; set; }

    public decimal Price { get; set; }
}

