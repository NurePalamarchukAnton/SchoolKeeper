using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.Models.Enums;

namespace SchoolKeeper.Pages;

public class RegisterModel : PageModel
{
    private readonly SchoolKeeperDbContext _context;

    public RegisterModel(SchoolKeeperDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public string FullName { get; set; } = string.Empty;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmPassword { get; set; } = string.Empty;

    [BindProperty]
    public int SchoolId { get; set; }

    [BindProperty]
    public UserRole Role { get; set; } = UserRole.Student;

    public List<SchoolInfo> Schools { get; set; } = new();

    public class SchoolInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Перевіряємо, чи користувач вже залогінений
        if (Request.Cookies.ContainsKey("authToken"))
        {
            return Redirect("/");
        }

        // Завантажуємо список шкіл
        Schools = await _context.Schools
            .Select(s => new SchoolInfo { Id = s.Id, Name = s.Name })
            .ToListAsync();
        
        return Page();
    }
}

