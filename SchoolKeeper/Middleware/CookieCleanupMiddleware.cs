namespace SchoolKeeper.Middleware;

public class CookieCleanupMiddleware
{
    private readonly RequestDelegate _next;

    public CookieCleanupMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Выполняем следующий middleware
        await _next(context);

        // Если получили 401 Unauthorized, очищаем cookie авторизации
        if (context.Response.StatusCode == 401)
        {
            // Очищаем все cookie связанные с авторизацией
            context.Response.Cookies.Delete("authToken");
            context.Response.Cookies.Delete("isImpersonating");
            context.Response.Cookies.Delete("originalAdminId");
        }
    }
}

