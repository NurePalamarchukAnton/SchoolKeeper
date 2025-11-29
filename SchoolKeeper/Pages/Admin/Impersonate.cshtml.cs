using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.Models;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Services;

namespace SchoolKeeper.Pages.Admin;

public class ImpersonateModel : PageModel
{
    private readonly SchoolKeeperDbContext _context;
    private readonly IUserService _userService;

    public ImpersonateModel(SchoolKeeperDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public bool IsImpersonating { get; set; }
    public string? ImpersonatedUserEmail { get; set; }
    public string? ImpersonatedUserRole { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null || user.Role != UserRole.Admin)
        {
            return Redirect("/Login");
        }

        // Перевіряємо, чи адмін вже працює від імені іншого користувача
        var isImpersonating = Request.Cookies.ContainsKey("isImpersonating") && 
                             Request.Cookies["isImpersonating"] == "true";
        
        if (isImpersonating)
        {
            var impersonatedUserId = _userService.GetCurrentUserId(HttpContext);
            if (impersonatedUserId.HasValue)
            {
                var impersonatedUser = await _userService.GetUserByIdAsync(impersonatedUserId.Value);
                if (impersonatedUser != null)
                {
                    IsImpersonating = true;
                    ImpersonatedUserEmail = impersonatedUser.Email;
                    ImpersonatedUserRole = impersonatedUser.Role.ToString();
                }
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostStopImpersonationAsync()
    {
        // Используем API endpoint для выхода из режима impersonation
        // Это позволит обновить localStorage на фронтенде
        // Перенаправляем на страницу, которая обработает выход через JavaScript
        return Redirect("/Admin/Impersonate?stop=true");
    }
}

