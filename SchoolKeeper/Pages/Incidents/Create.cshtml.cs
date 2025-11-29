using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.DTO;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Services;

namespace SchoolKeeper.Pages.Incidents;

public class CreateModel : PageModel
{
    private readonly SchoolKeeperDbContext _context;
    private readonly IUserService _userService;

    public CreateModel(SchoolKeeperDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    [BindProperty]
    public IncidentCreateDto Incident { get; set; } = new();

    public SelectList Devices { get; set; } = null!;
    public SelectList Severities { get; set; } = null!;
    public SelectList Statuses { get; set; } = null!;
    public bool IsAdmin { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        // Согласно матрице: только Admin и Security
        if (user == null || (user.Role != Models.Enums.UserRole.Admin && user.Role != Models.Enums.UserRole.Security))
        {
            return Redirect("/Login");
        }

        IsAdmin = user.Role == Models.Enums.UserRole.Admin;

        var devicesQuery = _context.Devices.AsQueryable();
        if (!IsAdmin)
        {
            devicesQuery = devicesQuery.Where(d => d.SchoolId == user.SchoolId);
        }

        Devices = new SelectList(await devicesQuery.ToListAsync(), "Id", "DeviceName");
        Severities = new SelectList(Enum.GetValues(typeof(IncidentSeverity)));
        Statuses = new SelectList(Enum.GetValues(typeof(IncidentStatus)));

        Incident.ReportedBy = user.Id;
        Incident.SchoolId = user.SchoolId;
        Incident.Timestamp = DateTime.UtcNow;
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        // Согласно матрице: только Admin и Security
        if (user == null || (user.Role != Models.Enums.UserRole.Admin && user.Role != Models.Enums.UserRole.Security))
        {
            return Redirect("/Login");
        }

        if (!ModelState.IsValid)
        {
            IsAdmin = user.Role == Models.Enums.UserRole.Admin;
            var devicesQuery = _context.Devices.AsQueryable();
            if (!IsAdmin)
            {
                devicesQuery = devicesQuery.Where(d => d.SchoolId == user.SchoolId);
            }
            Devices = new SelectList(await devicesQuery.ToListAsync(), "Id", "DeviceName");
            Severities = new SelectList(Enum.GetValues(typeof(IncidentSeverity)));
            Statuses = new SelectList(Enum.GetValues(typeof(IncidentStatus)));
            return Page();
        }

        // Non-admin users can only create incidents for their school
        if (user.Role != Models.Enums.UserRole.Admin && Incident.SchoolId != user.SchoolId)
        {
            ModelState.AddModelError("", "You can only create incidents for your school.");
            IsAdmin = false;
            var devicesQuery = _context.Devices.Where(d => d.SchoolId == user.SchoolId);
            Devices = new SelectList(await devicesQuery.ToListAsync(), "Id", "DeviceName");
            Severities = new SelectList(Enum.GetValues(typeof(IncidentSeverity)));
            Statuses = new SelectList(Enum.GetValues(typeof(IncidentStatus)));
            return Page();
        }

        Incident.ReportedBy = user.Id;
        if (!Incident.Timestamp.HasValue)
        {
            Incident.Timestamp = DateTime.UtcNow;
        }

        var incident = new Incident
        {
            DeviceId = Incident.DeviceId,
            ReportedBy = Incident.ReportedBy,
            IncidentType = Incident.IncidentType,
            Severity = Incident.Severity,
            Description = Incident.Description,
            Timestamp = Incident.Timestamp.Value,
            Status = Incident.Status,
            SchoolId = Incident.SchoolId
        };

        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}


