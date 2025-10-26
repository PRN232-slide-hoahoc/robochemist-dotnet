namespace RoboChemist.TemplateService.Model.Exceptions;

/// <summary>
/// Exception thrown when user is not authenticated
/// Maps to HTTP 401 Unauthorized
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }

    public UnauthorizedException() : base("User is not authenticated.")
    {
    }
}

