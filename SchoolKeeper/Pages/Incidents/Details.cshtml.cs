using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.DTO;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Services;

namespace SchoolKeeper.Pages.Incidents;

public class DetailsModel : PageModel
{
    private readonly SchoolKeeperDbContext _context;
    private readonly IUserService _userService;

    public DetailsModel(SchoolKeeperDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public IncidentDto Incident { get; set; } = null!;
    public string? DeviceName { get; set; }
    public string? ReporterName { get; set; }
    public bool CanEdit { get; set; }
    public bool CanResolve { get; set; }
    public bool CanManageUsers { get; set; }
    public List<UserInfo> IncidentUsers { get; set; } = new();
    public List<UserInfo> AvailableUsers { get; set; } = new();

    public class UserInfo
    {
        public int Id { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Role { get; set; } = default!;
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null)
        {
            return Redirect("/Login");
        }

        // Согласно матрице: Admin, Security, Teacher, Parent, Student
        if (user.Role != Models.Enums.UserRole.Admin && user.Role != Models.Enums.UserRole.Security && 
            user.Role != Models.Enums.UserRole.Teacher && user.Role != Models.Enums.UserRole.Parent && 
            user.Role != Models.Enums.UserRole.Student)
        {
            return Redirect("/Login");
        }

        var incident = await _context.Incidents
            .Include(i => i.Device)
            .Include(i => i.Reporter)
            .Include(i => i.UserIncidents)
                .ThenInclude(ui => ui.User)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (incident == null) return NotFound();

        // Check school access
        if (user.Role != Models.Enums.UserRole.Admin && incident.SchoolId != user.SchoolId)
        {
            return Unauthorized();
        }

        // Student может видеть только свои инциденты (где он участвует или создал)
        // Teacher может видеть все инциденты своей школы
        if (user.Role == Models.Enums.UserRole.Student)
        {
            var isParticipant = await _context.UserIncidents
                .AnyAsync(ui => ui.IncidentId == id && ui.UserId == user.Id);
            
            if (incident.ReportedBy != user.Id && !isParticipant)
            {
                return Unauthorized();
            }
        }

        Incident = new IncidentDto
        {
            Id = incident.Id,
            DeviceId = incident.DeviceId,
            ReportedBy = incident.ReportedBy,
            IncidentType = incident.IncidentType,
            Severity = incident.Severity,
            Description = incident.Description,
            Timestamp = incident.Timestamp,
            Status = incident.Status,
            SchoolId = incident.SchoolId
        };

        DeviceName = incident.Device?.DeviceName;
        ReporterName = incident.Reporter?.FullName;
        CanEdit = user.Role == Models.Enums.UserRole.Admin || user.Role == Models.Enums.UserRole.Security; // Teacher не может редактировать
        CanResolve = user.Role == Models.Enums.UserRole.Admin || user.Role == Models.Enums.UserRole.Teacher;
        CanManageUsers = user.Role == Models.Enums.UserRole.Admin || user.Role == Models.Enums.UserRole.Security || user.Role == Models.Enums.UserRole.Teacher;

        // Получаем пользователей, связанных с инцидентом
        IncidentUsers = incident.UserIncidents
            .Select(ui => new UserInfo
            {
                Id = ui.User.Id,
                FullName = ui.User.FullName,
                Email = ui.User.Email,
                Role = ui.User.Role.ToString()
            })
            .ToList();

        // Получаем доступных пользователей для добавления (из той же школы)
        var schoolIdForFilter = user.Role == Models.Enums.UserRole.Admin 
            ? incident.SchoolId 
            : user.SchoolId;

        var schoolUsersQuery = _context.Users
            .Where(u => u.Id != incident.ReportedBy);

        if (schoolIdForFilter.HasValue)
        {
            schoolUsersQuery = schoolUsersQuery.Where(u => u.SchoolId == schoolIdForFilter.Value);
        }

        var schoolUsers = await schoolUsersQuery.ToListAsync();

        var incidentUserIds = incident.UserIncidents.Select(ui => ui.UserId).ToList();
        AvailableUsers = schoolUsers
            .Where(u => !incidentUserIds.Contains(u.Id))
            .Select(u => new UserInfo
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role.ToString()
            })
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostResolveAsync(int id)
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null)
        {
            return Redirect("/Login");
        }

        // Согласно матрице: только Admin и Teacher
        if (user.Role != Models.Enums.UserRole.Admin && user.Role != Models.Enums.UserRole.Teacher)
        {
            return Redirect("/Login");
        }

        var incident = await _context.Incidents.FirstOrDefaultAsync(i => i.Id == id);
        if (incident == null)
        {
            return NotFound();
        }

        // Check school access
        if (user.Role != Models.Enums.UserRole.Admin && incident.SchoolId != user.SchoolId)
        {
            return Unauthorized();
        }

        // Teacher может решать все инциденты своей школы

        incident.Status = Models.Enums.IncidentStatus.Resolved;
        await _context.SaveChangesAsync();

        return RedirectToPage("./Details", new { id });
    }
}

