using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.DTO;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Response;
using SchoolKeeper.Services;

namespace SchoolKeeper.Pages.Devices;

// Убрали [Authorize] - проверяем авторизацию вручную
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
    public DeviceUpdateDto Device { get; set; } = null!;

    public SelectList DeviceTypes { get; set; } = null!;
    public SelectList DeviceStatuses { get; set; } = null!;
    public DeviceDto CurrentDevice { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var user = await _userService.GetCurrentUserAsync(HttpContext);
        // Согласно матрице: только Admin
        if (user == null || user.Role != UserRole.Admin)
        {
            return Redirect("/Login");
        }

        var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == id);
        if (device == null) return NotFound();

        // Check access
        if (user.Role != UserRole.Admin && device.SchoolId != user.SchoolId)
        {
            throw new UnauthorizedException("You don't have access to this device.");
        }

        CurrentDevice = new DeviceDto
        {
            Id = device.Id,
            DeviceName = device.DeviceName,
            DeviceType = device.DeviceType,
            Status = device.Status,
            Location = device.Location,
            SchoolId = device.SchoolId
        };

        Device = new DeviceUpdateDto
        {
            DeviceName = device.DeviceName,
            DeviceType = device.DeviceType,
            Status = device.Status,
            Location = device.Location,
            SchoolId = device.SchoolId
        };

        DeviceTypes = new SelectList(Enum.GetValues(typeof(DeviceType)));
        DeviceStatuses = new SelectList(Enum.GetValues(typeof(DeviceStatus)));

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        // Согласно матрице: только Admin
        if (user == null || user.Role != UserRole.Admin)
        {
            return Redirect("/Login");
        }

        var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == id);
        if (device == null) return NotFound();

        // Check access
        if (user.Role != UserRole.Admin && device.SchoolId != user.SchoolId)
        {
            throw new UnauthorizedException("You don't have access to this device.");
        }

        if (!ModelState.IsValid)
        {
            CurrentDevice = new DeviceDto
            {
                Id = device.Id,
                DeviceName = device.DeviceName,
                DeviceType = device.DeviceType,
                Status = device.Status,
                Location = device.Location,
                SchoolId = device.SchoolId
            };
            DeviceTypes = new SelectList(Enum.GetValues(typeof(DeviceType)));
            DeviceStatuses = new SelectList(Enum.GetValues(typeof(DeviceStatus)));
            return Page();
        }

        // Update device
        if (Device.DeviceName != null) device.DeviceName = Device.DeviceName;
        if (Device.DeviceType.HasValue) device.DeviceType = Device.DeviceType.Value;
        if (Device.Status.HasValue) device.Status = Device.Status.Value;
        if (Device.Location != null) device.Location = Device.Location;

        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}

