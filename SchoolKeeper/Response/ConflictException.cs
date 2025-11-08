namespace SchoolKeeper.Response;

/// <summary>
/// Exception for 409 Conflict responses
/// </summary>
public class ConflictException : ApiResponse
{
    public ConflictException(string? message = null, object? data = null)
        : base(409, message, data)
    {
    }
}

