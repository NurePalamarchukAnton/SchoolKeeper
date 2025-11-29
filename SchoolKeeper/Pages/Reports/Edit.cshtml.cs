using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.DTO;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Services;

namespace SchoolKeeper.Pages.Reports;

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
    public ReptUpdateDto Report { get; set; } = null!;

    public SelectList Schools { get; set; } = null!;
    public ReptDto CurrentReport { get; set; } = null!;
    public bool IsAdmin { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var user = await _userService.GetCurrentUserAsync(HttpContext);
        // Согласно матрице: Admin, Security, Teacher
        if (user == null || (user.Role != Models.Enums.UserRole.Admin && user.Role != Models.Enums.UserRole.Security && user.Role != Models.Enums.UserRole.Teacher))
        {
            return Redirect("/Login");
        }

        IsAdmin = user.Role == Models.Enums.UserRole.Admin;

        var report = await _context.Reports
            .Include(r => r.School)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null) return NotFound();

        // Check school access
        if (user.Role != UserRole.Admin && report.SchoolId != user.SchoolId)
        {
            return Unauthorized();
        }

        CurrentReport = new ReptDto
        {
            Id = report.Id,
            SchoolId = report.SchoolId,
            GeneratedBy = report.GeneratedBy,
            PeriodStart = report.PeriodStart,
            PeriodEnd = report.PeriodEnd,
            Summary = report.Summary,
            GeneratedOn = report.GeneratedOn
        };

        Report = new ReptUpdateDto
        {
            PeriodStart = report.PeriodStart,
            PeriodEnd = report.PeriodEnd,
            Summary = report.Summary
        };

        if (IsAdmin)
        {
            Schools = new SelectList(await _context.Schools.ToListAsync(), "Id", "Name", report.SchoolId);
        }
        else
        {
            Schools = new SelectList(await _context.Schools.Where(s => s.Id == user.SchoolId).ToListAsync(), "Id", "Name", report.SchoolId);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        // Согласно матрице: Admin, Security, Teacher
        if (user == null || (user.Role != Models.Enums.UserRole.Admin && user.Role != Models.Enums.UserRole.Security && user.Role != Models.Enums.UserRole.Teacher))
        {
            return Redirect("/Login");
        }

        var report = await _context.Reports.FirstOrDefaultAsync(r => r.Id == id);
        if (report == null) return NotFound();

        // Check school access
        if (user.Role != Models.Enums.UserRole.Admin && report.SchoolId != user.SchoolId)
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            IsAdmin = user.Role == Models.Enums.UserRole.Admin;
            CurrentReport = new ReptDto
            {
                Id = report.Id,
                SchoolId = report.SchoolId,
                GeneratedBy = report.GeneratedBy,
                PeriodStart = report.PeriodStart,
                PeriodEnd = report.PeriodEnd,
                Summary = report.Summary,
                GeneratedOn = report.GeneratedOn
            };

            if (IsAdmin)
            {
                Schools = new SelectList(await _context.Schools.ToListAsync(), "Id", "Name", report.SchoolId);
            }
            else
            {
                Schools = new SelectList(await _context.Schools.Where(s => s.Id == user.SchoolId).ToListAsync(), "Id", "Name", report.SchoolId);
            }
            return Page();
        }

        // Update report
        if (Report.PeriodStart.HasValue) report.PeriodStart = Report.PeriodStart.Value;
        if (Report.PeriodEnd.HasValue) report.PeriodEnd = Report.PeriodEnd.Value;
        if (Report.Summary != null) report.Summary = Report.Summary;
        if (Report.SchoolId.HasValue && IsAdmin) report.SchoolId = Report.SchoolId.Value;

        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}


