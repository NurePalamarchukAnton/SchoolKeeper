using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Services;

namespace SchoolKeeper.Pages.Parent;

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

    public int RelatedIncidents { get; set; }
    public int ActiveIncidents { get; set; }
    public int TotalReports { get; set; }
    public int SchoolId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null || (user.Role != UserRole.Parent && user.Role != UserRole.Teacher && user.Role != UserRole.Security && user.Role != UserRole.Admin))
        {
            return Redirect("/Login");
        }

        SchoolId = user.SchoolId;

        // For now, show all incidents in school (will be filtered by children later)
        RelatedIncidents = await _context.Incidents
            .CountAsync(i => i.SchoolId == SchoolId);
        ActiveIncidents = await _context.Incidents
            .CountAsync(i => i.SchoolId == SchoolId && i.StatusValue == IncidentStatus.Active.ToString());
        TotalReports = await _context.Reports
            .CountAsync(r => r.SchoolId == SchoolId);
        
        return Page();
    }
}

