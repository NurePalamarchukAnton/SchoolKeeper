using Microsoft.EntityFrameworkCore;
using SchoolKeeper.Models.Enums;
using System.Security.Claims;

namespace SchoolKeeper.Services;

public class UserService : IUserService
{
    private readonly SchoolKeeperDbContext _context;

    public UserService(SchoolKeeperDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetCurrentUserAsync(HttpContext httpContext)
    {
        var userId = GetCurrentUserId(httpContext);
        if (userId == null) return null;

        return await GetUserByIdAsync(userId.Value);
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.School)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public int? GetCurrentUserId(HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return null;

        return userId;
    }

    public string? GetCurrentUserRole(HttpContext httpContext)
    {
        return httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
    }

    public int? GetCurrentUserSchoolId(HttpContext httpContext)
    {
        var user = GetCurrentUserAsync(httpContext).Result;
        return user?.SchoolId;
    }

    public async Task<bool> HasAccessToSchoolAsync(HttpContext httpContext, int schoolId)
    {
        var userRole = GetCurrentUserRole(httpContext);
        
        // Admin has access to all schools
        if (userRole == "Admin")
            return true;

        var userSchoolId = GetCurrentUserSchoolId(httpContext);
        return userSchoolId == schoolId;
    }

    public async Task<bool> HasAccessToResourceAsync(HttpContext httpContext, int resourceSchoolId)
    {
        return await HasAccessToSchoolAsync(httpContext, resourceSchoolId);
    }
}

