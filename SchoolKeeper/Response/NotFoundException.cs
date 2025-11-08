namespace SchoolKeeper.Response;

/// <summary>
/// Exception for 404 Not Found responses
/// </summary>
public class NotFoundException : ApiResponse
{
    public NotFoundException(string? message = null, object? data = null)
        : base(404, message, data)
    {
    }
}

