namespace SchoolKeeper.Controllers
{
    public partial class AuthController
    {
        // ======== DTO модели ========
        public class RegisterDto
        {
            public string FullName { get; set; } = default!;
            public string Email { get; set; } = default!;
            public string Password { get; set; } = default!;
            public int SchoolId { get; set; }
            public UserRole Role { get; set; } = UserRole.Student;
        }
    }
}
