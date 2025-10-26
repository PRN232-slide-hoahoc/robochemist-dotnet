namespace RoboChemist.TemplateService.Model.DTOs;

/// <summary>
/// Standardized API response wrapper for all endpoints
/// Follows ROBOCHEMIST API coding standards
/// </summary>
/// <typeparam name="T">Type of data being returned</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message for the client (success or error message)
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The actual data payload
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// List of error messages (for validation errors)
    /// </summary>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// HTTP status code (additional metadata)
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Timestamp of the response (additional metadata)
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful response
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string message = "Success", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a failed response
    /// </summary>
    public static ApiResponse<T> FailResponse(string message, int statusCode = 400, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors,
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow
        };
    }
}

