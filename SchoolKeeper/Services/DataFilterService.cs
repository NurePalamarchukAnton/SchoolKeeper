using Microsoft.EntityFrameworkCore;
using SchoolKeeper.Models;
using SchoolKeeper.Models.Enums;

namespace SchoolKeeper.Services;

public class DataFilterService : IDataFilterService
{
    private readonly SchoolKeeperDbContext _context;

    public DataFilterService(SchoolKeeperDbContext context)
    {
        _context = context;
    }
    public IQueryable<T> FilterBySchool<T>(IQueryable<T> query, int? schoolId) where T : class
    {
        if (schoolId == null) return query;

        // Filter by SchoolId property if it exists
        var schoolIdProperty = typeof(T).GetProperty("SchoolId");
        if (schoolIdProperty != null)
        {
            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
            var property = System.Linq.Expressions.Expression.Property(parameter, schoolIdProperty);
            
            // Проверяем, является ли свойство nullable
            if (Nullable.GetUnderlyingType(schoolIdProperty.PropertyType) != null || schoolIdProperty.PropertyType == typeof(int?))
            {
                // Для nullable свойств сравниваем значение
                var constant = System.Linq.Expressions.Expression.Constant(schoolId.Value, typeof(int?));
                var equality = System.Linq.Expressions.Expression.Equal(property, constant);
                var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(equality, parameter);
                return query.Where(lambda);
            }
            else
            {
                // Для non-nullable свойств
                var constant = System.Linq.Expressions.Expression.Constant(schoolId.Value);
                var equality = System.Linq.Expressions.Expression.Equal(property, constant);
                var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(equality, parameter);
                return query.Where(lambda);
            }
        }

        return query;
    }

    public IQueryable<Incident> FilterIncidentsByRole(IQueryable<Incident> query, string? role, int? userId)
    {
        if (string.IsNullOrEmpty(role) || userId == null)
            return query;

        return role switch
        {
            "Admin" => query, // Admin sees all
            "Security" => query, // Security sees all in their school (already filtered by school)
            "Teacher" => query.Where(i => i.ReportedBy == userId), // Teacher sees their own
            "Parent" => GetParentIncidents(query, userId.Value), // Parent sees incidents related to their children
            "Student" => GetStudentIncidents(query, userId.Value), // Student sees incidents where they participate via UserIncident
            _ => query.Where(i => i.ReportedBy == userId) // Default: own incidents only
        };
    }

    public IQueryable<User> FilterUsersByRole(IQueryable<User> query, string? role, int? userId)
    {
        if (string.IsNullOrEmpty(role) || userId == null)
            return query;

        return role switch
        {
            "Admin" => query, // Admin sees all
            "Security" => query, // Security sees all in their school (already filtered by school)
            _ => query.Where(u => u.Id == userId) // Others see only themselves
        };
    }

    private IQueryable<Incident> GetParentIncidents(IQueryable<Incident> query, int parentId)
    {
        // Get all students (children) of this parent from ParentStudent table
        var childStudentIds = _context.ParentStudents
            .Where(ps => ps.ParentId == parentId)
            .Select(ps => ps.StudentId);

        // Get all incident IDs where children participate via UserIncident table
        var incidentIdsFromParticipation = _context.UserIncidents
            .Where(ui => childStudentIds.Contains(ui.UserId))
            .Select(ui => ui.IncidentId)
            .Distinct();

        // Get all incidents reported by children
        var incidentIdsFromReports = _context.Incidents
            .Where(i => childStudentIds.Contains(i.ReportedBy))
            .Select(i => i.Id)
            .Distinct();

        // Combine both sets of incident IDs and filter query
        // Include incidents where children participate OR where children reported
        return query.Where(i => 
            incidentIdsFromParticipation.Contains(i.Id) || 
            incidentIdsFromReports.Contains(i.Id));
    }

    private IQueryable<Incident> GetStudentIncidents(IQueryable<Incident> query, int studentId)
    {
        // Get incidents where student participates via UserIncident table
        var incidentIds = _context.UserIncidents
            .Where(ui => ui.UserId == studentId)
            .Select(ui => ui.IncidentId);

        return query.Where(i => incidentIds.Contains(i.Id));
    }

    public IQueryable<User> GetStudentTeachers(int studentId)
    {
        // Get teachers through StudentTeacher relationship
        var teacherIds = _context.StudentTeachers
            .Where(st => st.StudentId == studentId)
            .Select(st => st.TeacherId);

        return _context.Users
            .Where(u => teacherIds.Contains(u.Id) && u.RoleValue == UserRole.Teacher.ToString());
    }
}

