using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.DTO;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Services;

namespace SchoolKeeper.Pages.Incidents;

public class EditModel : PageModel
{
    private readonly SchoolKeeperDbContext _context;
    private readonly IUserService _userService;

    public EditModel(SchoolKeeperDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    [BindProperty]
    public IncidentUpdateDto Incident { get; set; } = null!;

    public SelectList Devices { get; set; } = null!;
    public SelectList Severities { get; set; } = null!;
    public SelectList Statuses { get; set; } = null!;
    public IncidentDto CurrentIncident { get; set; } = null!;
    public bool IsAdmin { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var user = await _userService.GetCurrentUserAsync(HttpContext);
        // Согласно матрице: Admin, Security и Teacher
        if (user == null || (user.Role != Models.Enums.UserRole.Admin && user.Role != Models.Enums.UserRole.Security && user.Role != Models.Enums.UserRole.Teacher))
        {
            return Redirect("/Login");
        }

        IsAdmin = user.Role == Models.Enums.UserRole.Admin;

        var incident = await _context.Incidents
            .Include(i => i.Device)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (incident == null) return NotFound();

        // Check school access
        if (user.Role != UserRole.Admin)
        {
            if (!incident.SchoolId.HasValue || incident.SchoolId.Value != user.SchoolId)
            {
                return Unauthorized();
            }
        }

        CurrentIncident = new IncidentDto
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

        Incident = new IncidentUpdateDto
        {
            IncidentType = incident.IncidentType,
            Severity = incident.Severity,
            Description = incident.Description,
            Status = incident.Status
        };

        var devicesQuery = _context.Devices.AsQueryable();
        if (!IsAdmin)
        {
            devicesQuery = devicesQuery.Where(d => d.SchoolId == user.SchoolId);
        }

        Devices = new SelectList(await devicesQuery.ToListAsync(), "Id", "DeviceName", incident.DeviceId);
        Severities = new SelectList(Enum.GetValues(typeof(IncidentSeverity)), incident.Severity);
        Statuses = new SelectList(Enum.GetValues(typeof(IncidentStatus)), incident.Status);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        // Согласно матрице: Admin, Security и Teacher
        if (user == null || (user.Role != Models.Enums.UserRole.Admin && user.Role != Models.Enums.UserRole.Security && user.Role != Models.Enums.UserRole.Teacher))
        {
            return Redirect("/Login");
        }

        var incident = await _context.Incidents.FirstOrDefaultAsync(i => i.Id == id);
        if (incident == null) return NotFound();

        // Check school access
        if (user.Role != Models.Enums.UserRole.Admin)
        {
            if (!incident.SchoolId.HasValue || incident.SchoolId.Value != user.SchoolId)
            {
                return Unauthorized();
            }
        }

        if (!ModelState.IsValid)
        {
            IsAdmin = user.Role == Models.Enums.UserRole.Admin;
            CurrentIncident = new IncidentDto
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

            var devicesQuery = _context.Devices.AsQueryable();
            if (!IsAdmin)
            {
                devicesQuery = devicesQuery.Where(d => d.SchoolId == user.SchoolId);
            }
            Devices = new SelectList(await devicesQuery.ToListAsync(), "Id", "DeviceName", incident.DeviceId);
            Severities = new SelectList(Enum.GetValues(typeof(IncidentSeverity)), incident.Severity);
            Statuses = new SelectList(Enum.GetValues(typeof(IncidentStatus)), incident.Status);
            return Page();
        }

        // Update incident
        if (Incident.IncidentType != null) incident.IncidentType = Incident.IncidentType;
        if (Incident.Severity.HasValue) incident.Severity = Incident.Severity.Value;
        if (Incident.Description != null) incident.Description = Incident.Description;
        if (Incident.Status.HasValue) incident.Status = Incident.Status.Value;
        if (Incident.DeviceId.HasValue) incident.DeviceId = Incident.DeviceId.Value;

        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}


