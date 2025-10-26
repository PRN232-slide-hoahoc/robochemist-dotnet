namespace RoboChemist.TemplateService.Model.Exceptions;

/// <summary>
/// Exception thrown when there is a conflict with existing data
/// Maps to HTTP 409 Conflict
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string name, object key) 
        : base($"{name} with id '{key}' already exists.")
    {
    }
}

