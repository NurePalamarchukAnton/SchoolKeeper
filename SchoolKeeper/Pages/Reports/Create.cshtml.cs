using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.DTO;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Services;

namespace SchoolKeeper.Pages.Reports;

public class CreateModel : PageModel
{
    private readonly SchoolKeeperDbContext _context;
    private readonly IUserService _userService;
    private readonly IReportGenerationService _reportGenerationService;

    public CreateModel(SchoolKeeperDbContext context, IUserService userService, IReportGenerationService reportGenerationService)
    {
        _context = context;
        _userService = userService;
        _reportGenerationService = reportGenerationService;
    }

    [BindProperty]
    public ReptCreateDto Report { get; set; } = new();

    public SelectList Schools { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        // Создание доступно только Teacher и Security
        if (user == null || (user.Role != Models.Enums.UserRole.Teacher && user.Role != Models.Enums.UserRole.Security))
        {
            return Redirect("/Login");
        }

        // Teacher и Security могут создавать отчеты только для своей школы
        Report.SchoolId = user.SchoolId;
        Schools = new SelectList(await _context.Schools.Where(s => s.Id == user.SchoolId).ToListAsync(), "Id", "Name");

        Report.GeneratedBy = user.Id;
        Report.GeneratedOn = DateTime.UtcNow;
        Report.PeriodStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        Report.PeriodEnd = DateOnly.FromDateTime(DateTime.UtcNow);
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        // Создание доступно только Teacher и Security
        if (user == null || (user.Role != Models.Enums.UserRole.Teacher && user.Role != Models.Enums.UserRole.Security))
        {
            return Redirect("/Login");
        }

        if (!ModelState.IsValid)
        {
            Schools = new SelectList(await _context.Schools.Where(s => s.Id == user.SchoolId).ToListAsync(), "Id", "Name");
            return Page();
        }

        // Teacher и Security могут создавать отчеты только для своей школы
        if (Report.SchoolId != user.SchoolId)
        {
            ModelState.AddModelError("", "You can only create reports for your school.");
            Schools = new SelectList(await _context.Schools.Where(s => s.Id == user.SchoolId).ToListAsync(), "Id", "Name");
            return Page();
        }

        Report.GeneratedBy = user.Id;
        if (!Report.GeneratedOn.HasValue)
        {
            Report.GeneratedOn = DateTime.UtcNow;
        }

        // Автоматическая генерация Summary на основе роли
        string generatedSummary;
        if (user.Role == Models.Enums.UserRole.Teacher)
        {
            generatedSummary = await _reportGenerationService.GenerateTeacherReportAsync(
                Report.SchoolId, 
                Report.PeriodStart, 
                Report.PeriodEnd, 
                user.Id);
        }
        else // Security
        {
            generatedSummary = await _reportGenerationService.GenerateSecurityReportAsync(
                Report.SchoolId, 
                Report.PeriodStart, 
                Report.PeriodEnd);
        }

        // Если пользователь ввел свой Summary, добавляем его, иначе используем сгенерированный
        var finalSummary = !string.IsNullOrWhiteSpace(Report.Summary) 
            ? $"{generatedSummary}\n\nДОДАТКОВА ІНФОРМАЦІЯ:\n{Report.Summary}"
            : generatedSummary;

        var report = new Rept
        {
            SchoolId = Report.SchoolId,
            GeneratedBy = Report.GeneratedBy,
            PeriodStart = Report.PeriodStart,
            PeriodEnd = Report.PeriodEnd,
            Summary = finalSummary,
            GeneratedOn = Report.GeneratedOn.Value
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}


