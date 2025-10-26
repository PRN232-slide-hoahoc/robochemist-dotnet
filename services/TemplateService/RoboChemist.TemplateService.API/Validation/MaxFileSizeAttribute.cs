using System.ComponentModel.DataAnnotations;

namespace RoboChemist.TemplateService.API.Validation;

/// <summary>
/// Validates that the uploaded file does not exceed the maximum size
/// </summary>
public class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly long _maxFileSize;

    public MaxFileSizeAttribute(long maxFileSize)
    {
        _maxFileSize = maxFileSize;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            if (file.Length > _maxFileSize)
            {
                var maxSizeInMB = _maxFileSize / (1024 * 1024);
                return new ValidationResult(
                    $"File size must not exceed {maxSizeInMB}MB");
            }
        }

        return ValidationResult.Success;
    }
}

