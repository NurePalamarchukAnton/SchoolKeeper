using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.DTO;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Services;

namespace SchoolKeeper.Pages.Incidents;

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

    public List<IncidentDto> Incidents { get; set; } = new();
    public string? UserRole { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanResolve { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
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

        UserRole = user.Role.ToString();
        CanCreate = user.Role == Models.Enums.UserRole.Admin; // Только Admin может создавать инциденты
        CanEdit = user.Role == Models.Enums.UserRole.Admin || user.Role == Models.Enums.UserRole.Security; // Teacher не может редактировать
        CanResolve = user.Role == Models.Enums.UserRole.Admin || user.Role == Models.Enums.UserRole.Teacher;

        var query = _context.Incidents
            .Include(i => i.Device)
            .Include(i => i.Reporter)
            .AsQueryable();

        // Filter by school (except Admin)
        if (user.Role != Models.Enums.UserRole.Admin)
        {
            query = _dataFilterService.FilterBySchool(query, user.SchoolId);
        }

        // Apply role-specific filtering using DataFilterService
        query = _dataFilterService.FilterIncidentsByRole(query, user.Role.ToString(), user.Id);

        var incidents = await query
            .OrderByDescending(i => i.Timestamp)
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
        if (user.Role != Models.Enums.UserRole.Admin)
        {
            if (!incident.SchoolId.HasValue || incident.SchoolId.Value != user.SchoolId)
            {
                return Unauthorized();
            }
        }

        // Teacher может решать все инциденты своей школы

        incident.Status = Models.Enums.IncidentStatus.Resolved;
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}

