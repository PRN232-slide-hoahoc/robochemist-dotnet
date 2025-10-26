namespace RoboChemist.TemplateService.Model.Exceptions;

/// <summary>
/// Exception thrown when the request is invalid
/// Maps to HTTP 400 Bad Request
/// </summary>
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
    }

    public BadRequestException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

