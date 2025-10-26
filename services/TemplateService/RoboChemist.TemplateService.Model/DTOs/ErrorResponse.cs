namespace RoboChemist.TemplateService.Model.DTOs;

/// <summary>
/// Standardized error response for API
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Error message for client
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Detailed error information (only in Development)
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Timestamp when error occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Validation errors (for 400 Bad Request)
    /// </summary>
    public IDictionary<string, string[]>? Errors { get; set; }
}

