using System.Net;
using System.Text.Json;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.TemplateService.Model.Exceptions;

namespace RoboChemist.TemplateService.API.Middleware;

/// <summary>
/// Global exception handling middleware
/// Catches all unhandled exceptions and returns standardized error responses
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        string message;
        List<string>? errors = null;

        switch (exception)
        {
            case NotFoundException notFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                message = notFoundEx.Message;
                break;

            case BadRequestException badRequestEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                message = badRequestEx.Message;
                break;

            case UnauthorizedException unauthorizedEx:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                message = unauthorizedEx.Message;
                break;

            case ForbiddenException forbiddenEx:
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                message = forbiddenEx.Message;
                break;

            case ConflictException conflictEx:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                message = conflictEx.Message;
                break;

            default:
                // Generic 500 Internal Server Error
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                message = "An internal server error occurred. Please try again later.";
                
                // Only show detailed error in Development environment
                if (_environment.IsDevelopment())
                {
                    errors = new List<string> { exception.ToString() };
                }
                break;
        }

        var errorResponse = ApiResponse<object>.ErrorResult(message, errors);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment() // Pretty print in Development
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// Extension method to register the middleware
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}

