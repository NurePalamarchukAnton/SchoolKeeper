using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SchoolKeeper.Services;

namespace SchoolKeeper.Pages;

// Убрали [Authorize] - страница доступна всем
public class IndexModel : PageModel
{
    private readonly IUserService _userService;

    public IndexModel(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Всегда показываем главную страницу
        // Редирект на страницы по ролям происходит только через JavaScript после логина
        return Page();
    }
}

