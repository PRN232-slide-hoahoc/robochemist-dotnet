using System.ComponentModel.DataAnnotations;

namespace RoboChemist.TemplateService.API.Validation;

/// <summary>
/// Validates that the uploaded file has an allowed extension
/// </summary>
public class AllowedFileExtensionsAttribute : ValidationAttribute
{
    private readonly string[] _extensions;

    public AllowedFileExtensionsAttribute(params string[] extensions)
    {
        _extensions = extensions;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!_extensions.Contains(extension))
            {
                return new ValidationResult(
                    $"Only the following file extensions are allowed: {string.Join(", ", _extensions)}");
            }
        }

        return ValidationResult.Success;
    }
}

