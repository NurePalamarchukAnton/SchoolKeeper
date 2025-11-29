using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.DTO;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Services;

namespace SchoolKeeper.Pages.Devices;

// Убрали [Authorize] - проверяем авторизацию вручную
public class CreateModel : PageModel
{
    private readonly SchoolKeeperDbContext _context;
    private readonly IUserService _userService;

    public CreateModel(SchoolKeeperDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    [BindProperty]
    public DeviceCreateDto Device { get; set; } = new();

    public SelectList DeviceTypes { get; set; } = null!;
    public SelectList DeviceStatuses { get; set; } = null!;
    public SelectList Schools { get; set; } = null!;
    public int? UserSchoolId { get; set; }
    public bool IsAdmin { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        // Согласно матрице: только Admin
        if (user == null || user.Role != UserRole.Admin)
        {
            return Redirect("/Login");
        }

        IsAdmin = user.Role == UserRole.Admin;
        UserSchoolId = user.SchoolId;

        DeviceTypes = new SelectList(Enum.GetValues(typeof(DeviceType)));
        DeviceStatuses = new SelectList(Enum.GetValues(typeof(DeviceStatus)));

        if (IsAdmin)
        {
            Schools = new SelectList(await _context.Schools.ToListAsync(), "Id", "Name");
        }
        else
        {
            Device.SchoolId = user.SchoolId;
            Schools = new SelectList(await _context.Schools.Where(s => s.Id == user.SchoolId).ToListAsync(), "Id", "Name");
        }
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        // Согласно матрице: только Admin
        if (user == null || user.Role != UserRole.Admin)
        {
            return Redirect("/Login");
        }

        if (!IsAdmin && Device.SchoolId != user.SchoolId)
        {
            ModelState.AddModelError("", "You can only create devices for your school.");
            await OnGetAsync();
            return Page();
        }

        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var device = new Device
        {
            DeviceName = Device.DeviceName,
            DeviceType = Device.DeviceType,
            Status = Device.Status,
            Location = Device.Location,
            SchoolId = Device.SchoolId
        };

        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}

