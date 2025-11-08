namespace SchoolKeeper.Response;

/// <summary>
/// Exception for 401 Unauthorized responses
/// </summary>
public class UnauthorizedException : ApiResponse
{
    public UnauthorizedException(string? message = null, object? data = null)
        : base(401, message, data)
    {
    }
}

