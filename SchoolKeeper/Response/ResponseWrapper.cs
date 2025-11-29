namespace SchoolKeeper.Response;

/// <summary>
/// Wrapper class for successful API responses
/// </summary>
public class ResponseWrapper<T>
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public ResponseWrapper(int statusCode, T? data = default, string? message = null)
    {
        StatusCode = statusCode;
        Data = data;
        Message = message ?? GetDefaultMessage(statusCode);
    }

    private static string GetDefaultMessage(int statusCode) => statusCode switch
    {
        200 => "Success",
        201 => "Created",
        204 => "No Content",
        _ => "Success"
    };
}

