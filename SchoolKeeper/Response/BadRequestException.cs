namespace SchoolKeeper.Response;

/// <summary>
/// Exception for 400 Bad Request responses
/// </summary>
public class BadRequestException : ApiResponse
{
    public BadRequestException(string? message = null, object? data = null)
        : base(400, message, data)
    {
    }
}

