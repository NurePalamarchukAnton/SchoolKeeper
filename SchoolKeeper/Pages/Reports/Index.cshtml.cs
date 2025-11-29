using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.DTO;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Services;

namespace SchoolKeeper.Pages.Reports;

public class IndexModel : PageModel
{
    private readonly SchoolKeeperDbContext _context;
    private readonly IUserService _userService;

    public IndexModel(SchoolKeeperDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public List<ReportInfo> Reports { get; set; } = new();
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }

    public class ReportInfo
    {
        public int Id { get; set; }
        public DateOnly PeriodStart { get; set; }
        public DateOnly PeriodEnd { get; set; }
        public DateTime GeneratedOn { get; set; }
        public string GeneratorName { get; set; } = default!;
    }

    public async Task<IActionResult> OnGetAsync()
    {
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

        // Создание доступно только Teacher и Security
        CanCreate = user.Role == Models.Enums.UserRole.Teacher || user.Role == Models.Enums.UserRole.Security;
        CanEdit = false; // Отчеты не редактируются после создания

        var query = _context.Reports
            .Include(r => r.School)
            .Include(r => r.Generator)
            .AsQueryable();

        // Filter by school (except Admin)
        if (user.Role != Models.Enums.UserRole.Admin)
        {
            query = query.Where(r => r.SchoolId == user.SchoolId);
        }

        var reports = await query
            .OrderByDescending(r => r.GeneratedOn)
            .ToListAsync();

        Reports = reports.Select(r => new ReportInfo
        {
            Id = r.Id,
            PeriodStart = r.PeriodStart,
            PeriodEnd = r.PeriodEnd,
            GeneratedOn = r.GeneratedOn,
            GeneratorName = r.Generator?.FullName ?? "Невідомий"
        }).ToList();
        
        return Page();
    }
}


