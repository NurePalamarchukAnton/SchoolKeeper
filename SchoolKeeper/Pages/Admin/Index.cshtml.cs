using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Services;

namespace SchoolKeeper.Pages.Admin;

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

    public int TotalSchools { get; set; }
    public int TotalUsers { get; set; }
    public int TotalDevices { get; set; }
    public int ActiveIncidents { get; set; }
    public int ResolvedIncidents { get; set; }
    public int TotalReports { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null || user.Role != UserRole.Admin)
        {
            return Redirect("/Login");
        }

        TotalSchools = await _context.Schools.CountAsync();
        TotalUsers = await _context.Users.CountAsync();
        TotalDevices = await _context.Devices.CountAsync();
        ActiveIncidents = await _context.Incidents
            .CountAsync(i => i.StatusValue == IncidentStatus.Active.ToString());
        ResolvedIncidents = await _context.Incidents
            .CountAsync(i => i.StatusValue == IncidentStatus.Resolved.ToString());
        TotalReports = await _context.Reports.CountAsync();
        
        return Page();
    }
}

