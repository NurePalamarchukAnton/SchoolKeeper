using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.Models.Enums;

namespace SchoolKeeper.Pages.Schools;

public class IndexModel : PageModel
{
    private readonly SchoolKeeperDbContext _context;

    public IndexModel(SchoolKeeperDbContext context)
    {
        _context = context;
    }

    public List<SchoolInfo> Schools { get; set; } = new();

    public class SchoolInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Address { get; set; }
        public string? Region { get; set; }
        public string? ContactNumber { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Страница доступна всем без авторизации
        // Получаем все школы, подключенные к системе
        Schools = await _context.Schools
            .Select(s => new SchoolInfo
            {
                Id = s.Id,
                Name = s.Name,
                Address = s.Address,
                Region = s.Region,
                ContactNumber = s.ContactNumber
            })
            .OrderBy(s => s.Name)
            .ToListAsync();

        return Page();
    }
}

