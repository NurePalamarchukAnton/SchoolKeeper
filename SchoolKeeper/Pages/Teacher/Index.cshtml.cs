using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.DTO;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Services;

namespace SchoolKeeper.Pages.Teacher;

// Убрали [Authorize] - проверяем авторизацию вручную
public class IndexModel : PageModel
{
    private readonly SchoolKeeperDbContext _context;
    private readonly IUserService _userService;

    public IndexModel(SchoolKeeperDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public List<StudentInfo> Students { get; set; } = new();
    public List<ParentInfo> Parents { get; set; } = new();
    public List<IncidentDto> Incidents { get; set; } = new();
    public int SchoolId { get; set; }

    public class StudentInfo
    {
        public int Id { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? PhoneNumber { get; set; }
    }

    public class ParentInfo
    {
        public int Id { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public List<string> ChildrenNames { get; set; } = new();
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null || user.Role != UserRole.Teacher)
        {
            return Redirect("/Login");
        }

        SchoolId = user.SchoolId;

        // Получаем студентов учителя через StudentTeacher
        var studentIds = await _context.StudentTeachers
            .Where(st => st.TeacherId == user.Id)
            .Select(st => st.StudentId)
            .ToListAsync();

        // Получаем информацию о студентах
        Students = await _context.Users
            .Where(u => studentIds.Contains(u.Id))
            .Select(u => new StudentInfo
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber
            })
            .ToListAsync();

        // Получаем родителей студентов через ParentStudent
        var parentIds = await _context.ParentStudents
            .Where(ps => studentIds.Contains(ps.StudentId))
            .Select(ps => ps.ParentId)
            .Distinct()
            .ToListAsync();

        // Получаем информацию о родителях с их детьми
        var parentsData = await _context.Users
            .Where(u => parentIds.Contains(u.Id))
            .Include(u => u.ParentRelationships)
                .ThenInclude(ps => ps.Student)
            .ToListAsync();

        Parents = parentsData.Select(p => new ParentInfo
        {
            Id = p.Id,
            FullName = p.FullName,
            Email = p.Email,
            PhoneNumber = p.PhoneNumber,
            ChildrenNames = p.ParentRelationships
                .Where(ps => studentIds.Contains(ps.StudentId))
                .Select(ps => ps.Student.FullName)
                .ToList()
        }).ToList();

        // Получаем все инциденты школы учителя
        // Учитель должен видеть все инциденты своей школы, а не только связанные со студентами
        var incidents = await _context.Incidents
            .Where(i => i.SchoolId == SchoolId)
            .Include(i => i.Device)
            .Include(i => i.Reporter)
            .OrderByDescending(i => i.Timestamp)
            .Take(50)
            .ToListAsync();

        Incidents = incidents.Select(i => new IncidentDto
        {
            Id = i.Id,
            DeviceId = i.DeviceId,
            ReportedBy = i.ReportedBy,
            IncidentType = i.IncidentType,
            Severity = i.Severity,
            Description = i.Description,
            Timestamp = i.Timestamp,
            Status = i.Status,
            SchoolId = i.SchoolId
        }).ToList();
        
        return Page();
    }
}

