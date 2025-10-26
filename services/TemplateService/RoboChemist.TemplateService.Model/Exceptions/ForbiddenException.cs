namespace RoboChemist.TemplateService.Model.Exceptions;

/// <summary>
/// Exception thrown when user doesn't have permission
/// Maps to HTTP 403 Forbidden
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }

    public ForbiddenException() : base("You don't have permission to access this resource.")
    {
    }
}

