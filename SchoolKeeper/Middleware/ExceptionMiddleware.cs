using SchoolKeeper.Response;
using System.Text.Json;

namespace SchoolKeeper.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _environment;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionMiddleware(RequestDelegate next, IHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip exception middleware for Swagger/OpenAPI endpoints to allow proper error pages in development
        if (context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/openapi"))
        {
            await _next(context);
            return;
        }

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        if (ex is not ApiResponse apiResponse)
        {
            // In development, let ASP.NET Core handle non-ApiResponse exceptions
            // to show the developer exception page
            if (_environment.IsDevelopment())
            {
                throw ex;
            }

            // In production, return a generic error for non-ApiResponse exceptions
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;
            var genericError = new
            {
                statusCode = 500,
                message = "An internal server error occurred."
            };
            string json = JsonSerializer.Serialize(genericError, SerializerOptions);
            await context.Response.WriteAsync(json);
            return;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = apiResponse.StatusCode;
        var response = new
        {
            statusCode = apiResponse.StatusCode,
            message = apiResponse.Message,
            data = apiResponse.Data
        };
        string json2 = JsonSerializer.Serialize(response, SerializerOptions);
        await context.Response.WriteAsync(json2);
    }
}

