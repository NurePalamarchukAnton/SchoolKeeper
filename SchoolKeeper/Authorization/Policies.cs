using Microsoft.AspNetCore.Authorization;

namespace SchoolKeeper.Authorization;

public static class Policies
{
    public const string AdminOnly = "AdminOnly";
    public const string SecurityOrAdmin = "SecurityOrAdmin";
    public const string TeacherOrAbove = "TeacherOrAbove";
    public const string ParentOrAbove = "ParentOrAbove";
    public const string AllAuthenticated = "AllAuthenticated";

    public static void ConfigurePolicies(AuthorizationOptions options)
    {
        // Admin only
        options.AddPolicy(AdminOnly, policy =>
            policy.RequireRole("Admin"));

        // Security or Admin
        options.AddPolicy(SecurityOrAdmin, policy =>
            policy.RequireRole("Admin", "Security"));

        // Teacher, Security, or Admin
        options.AddPolicy(TeacherOrAbove, policy =>
            policy.RequireRole("Admin", "Security", "Teacher"));

        // Parent, Teacher, Security, or Admin
        options.AddPolicy(ParentOrAbove, policy =>
            policy.RequireRole("Admin", "Security", "Teacher", "Parent"));

        // All authenticated users
        options.AddPolicy(AllAuthenticated, policy =>
            policy.RequireAuthenticatedUser());
    }
}

