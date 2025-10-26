namespace RoboChemist.TemplateService.Model.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found
/// Maps to HTTP 404 Not Found
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string name, object key) 
        : base($"{name} with id '{key}' was not found.")
    {
    }
}

