using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.DTO;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Response;
using SchoolKeeper.Services;

namespace SchoolKeeper.Pages.Devices;

// Убрали [Authorize] - проверяем авторизацию вручную
public class DetailsModel : PageModel
{
    private readonly SchoolKeeperDbContext _context;
    private readonly IUserService _userService;

    public DetailsModel(SchoolKeeperDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public DeviceDto Device { get; set; } = null!;
    public bool CanEdit { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var user = await _userService.GetCurrentUserAsync(HttpContext);
        // Согласно матрице: только Admin и Security
        if (user == null || (user.Role != UserRole.Admin && user.Role != UserRole.Security))
        {
            return Redirect("/Login");
        }

        var device = await _context.Devices
            .Include(d => d.School)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (device == null) return NotFound();

        // Check access
        if (user.Role != UserRole.Admin && device.SchoolId != user.SchoolId)
        {
            throw new UnauthorizedException("You don't have access to this device.");
        }

        Device = new DeviceDto
        {
            Id = device.Id,
            DeviceName = device.DeviceName,
            DeviceType = device.DeviceType,
            Status = device.Status,
            Location = device.Location,
            SchoolId = device.SchoolId
        };

        CanEdit = user.Role == UserRole.Admin; // Согласно матрице: только Admin может редактировать

        return Page();
    }
}

