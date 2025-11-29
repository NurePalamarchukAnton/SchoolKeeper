using SchoolKeeper.Models;

namespace SchoolKeeper.Services;

public interface IUserService
{
    Task<User?> GetCurrentUserAsync(HttpContext httpContext);
    Task<User?> GetUserByIdAsync(int userId);
    int? GetCurrentUserId(HttpContext httpContext);
    string? GetCurrentUserRole(HttpContext httpContext);
    int? GetCurrentUserSchoolId(HttpContext httpContext);
    Task<bool> HasAccessToSchoolAsync(HttpContext httpContext, int schoolId);
    Task<bool> HasAccessToResourceAsync(HttpContext httpContext, int resourceSchoolId);
}

