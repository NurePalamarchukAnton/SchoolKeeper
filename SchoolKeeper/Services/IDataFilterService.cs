using SchoolKeeper.Models;

namespace SchoolKeeper.Services;

public interface IDataFilterService
{
    IQueryable<T> FilterBySchool<T>(IQueryable<T> query, int? schoolId) where T : class;
    IQueryable<Incident> FilterIncidentsByRole(IQueryable<Incident> query, string? role, int? userId);
    IQueryable<User> FilterUsersByRole(IQueryable<User> query, string? role, int? userId);
    IQueryable<User> GetStudentTeachers(int studentId);
}

