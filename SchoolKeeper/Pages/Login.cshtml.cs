using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SchoolKeeper.Pages;

public class LoginModel : PageModel
{
    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        // Перевіряємо, чи користувач вже залогінений
        if (Request.Cookies.ContainsKey("authToken"))
        {
            return Redirect("/");
        }
        return Page();
    }
}

