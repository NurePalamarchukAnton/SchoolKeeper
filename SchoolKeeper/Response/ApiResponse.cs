namespace SchoolKeeper.Response;

/// <summary>
/// Base exception class for API responses with status codes
/// </summary>
public class ApiResponse : Exception
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }

    public ApiResponse(int statusCode, string? message = null, object? data = null)
        : base(message ?? GetDefaultMessage(statusCode))
    {
        StatusCode = statusCode;
        Message = message ?? GetDefaultMessage(statusCode);
        Data = data;
    }

    private static string GetDefaultMessage(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        422 => "Unprocessable Entity",
        500 => "Internal Server Error",
        _ => "An error occurred"
    };
}

