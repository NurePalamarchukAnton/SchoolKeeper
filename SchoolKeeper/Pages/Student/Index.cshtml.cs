using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.DTO;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Services;

namespace SchoolKeeper.Pages.Student;

// Убрали [Authorize] - проверяем авторизацию вручную
public class IndexModel : PageModel
{
    private readonly SchoolKeeperDbContext _context;
    private readonly IUserService _userService;
    private readonly IDataFilterService _dataFilterService;

    public IndexModel(SchoolKeeperDbContext context, IUserService userService, IDataFilterService dataFilterService)
    {
        _context = context;
        _userService = userService;
        _dataFilterService = dataFilterService;
    }

    public List<UserDto> Teachers { get; set; } = new();
    public List<IncidentDto> MyIncidents { get; set; } = new();
    public int ActiveIncidentsCount { get; set; }
    public int TotalIncidentsCount { get; set; }
    public int SchoolId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null || user.Role != UserRole.Student)
        {
            return Redirect("/Login");
        }

        SchoolId = user.SchoolId;

        // Загружаем учителей студента через StudentTeacher
        var teachersQuery = _dataFilterService.GetStudentTeachers(user.Id);
        var teachers = await teachersQuery
            .Include(u => u.School)
            .ToListAsync();

        Teachers = teachers.Select(t => new UserDto
        {
            Id = t.Id,
            FullName = t.FullName,
            Email = t.Email,
            Role = t.Role,
            PhoneNumber = t.PhoneNumber,
            SchoolId = t.SchoolId
        }).ToList();

        // Загружаем инциденты, где студент участвует через UserIncident
        var incidentIds = await _context.UserIncidents
            .Where(ui => ui.UserId == user.Id)
            .Select(ui => ui.IncidentId)
            .ToListAsync();

        var incidentsQuery = _context.Incidents
            .Include(i => i.Device)
            .Include(i => i.Reporter)
            .Where(i => incidentIds.Contains(i.Id));

        // Фильтруем по школе
        incidentsQuery = incidentsQuery.Where(i => i.SchoolId == user.SchoolId);

        var incidents = await incidentsQuery
            .OrderByDescending(i => i.Timestamp)
            .ToListAsync();

        MyIncidents = incidents.Select(i => new IncidentDto
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

        TotalIncidentsCount = MyIncidents.Count;
        ActiveIncidentsCount = MyIncidents.Count(i => i.Status == IncidentStatus.Active);
        
        return Page();
    }
}

