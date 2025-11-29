using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Services;

namespace SchoolKeeper.Pages.Security;

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

    public int TotalDevices { get; set; }
    public int ActiveDevices { get; set; }
    public int InactiveDevices { get; set; }
    public int ActiveIncidents { get; set; }
    public int ResolvedIncidents { get; set; }
    public int TotalReports { get; set; }
    public int SchoolId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null || (user.Role != UserRole.Security && user.Role != UserRole.Admin))
        {
            return Redirect("/Login");
        }

        SchoolId = user.SchoolId;

        TotalDevices = await _context.Devices
            .CountAsync(d => d.SchoolId == SchoolId);
        ActiveDevices = await _context.Devices
            .CountAsync(d => d.SchoolId == SchoolId && d.StatusValue == DeviceStatus.Active.ToString());
        InactiveDevices = await _context.Devices
            .CountAsync(d => d.SchoolId == SchoolId && d.StatusValue != DeviceStatus.Active.ToString());
        ActiveIncidents = await _context.Incidents
            .CountAsync(i => i.SchoolId == SchoolId && i.StatusValue == IncidentStatus.Active.ToString());
        ResolvedIncidents = await _context.Incidents
            .CountAsync(i => i.SchoolId == SchoolId && i.StatusValue == IncidentStatus.Resolved.ToString());
        TotalReports = await _context.Reports
            .CountAsync(r => r.SchoolId == SchoolId);
        
        return Page();
    }
}

