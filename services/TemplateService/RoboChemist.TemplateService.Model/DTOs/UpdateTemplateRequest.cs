using System.ComponentModel.DataAnnotations;

namespace RoboChemist.TemplateService.Model.DTOs;

public class UpdateTemplateRequest
{
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

    public bool IsActive { get; set; } = true;
}
