using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.DTO;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Services;

namespace SchoolKeeper.Pages.Devices;

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

    public List<DeviceDto> Devices { get; set; } = new();
    public string? UserRole { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null)
        {
            return Redirect("/Login");
        }

        // Согласно матрице: только Admin и Security
        if (user.Role != Models.Enums.UserRole.Admin && user.Role != Models.Enums.UserRole.Security)
        {
            return Redirect("/Login");
        }

        UserRole = user.Role.ToString();
        CanCreate = user.Role == Models.Enums.UserRole.Admin; // Согласно матрице: только Admin
        CanEdit = user.Role == Models.Enums.UserRole.Admin; // Согласно матрице: только Admin

        var query = _context.Devices
            .Include(d => d.School)
            .AsQueryable();

        if (user.Role != Models.Enums.UserRole.Admin)
        {
            query = query.Where(d => d.SchoolId == user.SchoolId);
        }

        var devices = await query
            .OrderBy(d => d.DeviceName)
            .ToListAsync();

        Devices = devices.Select(d => new DeviceDto
        {
            Id = d.Id,
            DeviceName = d.DeviceName,
            DeviceType = d.DeviceType,
            Status = d.Status,
            Location = d.Location,
            SchoolId = d.SchoolId
        }).ToList();
        
        return Page();
    }
}

