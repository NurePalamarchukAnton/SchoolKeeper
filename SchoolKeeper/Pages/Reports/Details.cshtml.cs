using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.DTO;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Services;

namespace SchoolKeeper.Pages.Reports;

public class DetailsModel : PageModel
{
    private readonly SchoolKeeperDbContext _context;
    private readonly IUserService _userService;

    public DetailsModel(SchoolKeeperDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public ReptDto Report { get; set; } = null!;
    public string? SchoolName { get; set; }
    public string? GeneratorName { get; set; }
    public bool CanEdit { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null)
        {
            return Redirect("/Login");
        }

        // Просмотр доступен всем ролям: Admin, Security, Teacher, Parent, Student
        if (user.Role != Models.Enums.UserRole.Admin && user.Role != Models.Enums.UserRole.Security && 
            user.Role != Models.Enums.UserRole.Teacher && user.Role != Models.Enums.UserRole.Parent && 
            user.Role != Models.Enums.UserRole.Student)
        {
            return Redirect("/Login");
        }

        var report = await _context.Reports
            .Include(r => r.School)
            .Include(r => r.Generator)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null) return NotFound();

        // Check school access (except Admin)
        if (user.Role != Models.Enums.UserRole.Admin && report.SchoolId != user.SchoolId)
        {
            return Unauthorized();
        }

        Report = new ReptDto
        {
            Id = report.Id,
            SchoolId = report.SchoolId,
            GeneratedBy = report.GeneratedBy,
            PeriodStart = report.PeriodStart,
            PeriodEnd = report.PeriodEnd,
            Summary = report.Summary,
            GeneratedOn = report.GeneratedOn
        };

        SchoolName = report.School?.Name;
        GeneratorName = report.Generator?.FullName;
        CanEdit = false; // Отчеты не редактируются после создания

        return Page();
    }
}


