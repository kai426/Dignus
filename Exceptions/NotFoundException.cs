namespace Dignus.Candidate.Back.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, Guid id)
        : base($"{entityName} with ID {id} not found")
    {
    }
}
