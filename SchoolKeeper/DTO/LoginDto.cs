namespace SchoolKeeper.Controllers
{
    public partial class AuthController
    {
        public class LoginDto
        {
            public string Email { get; set; } = default!;
            public string Password { get; set; } = default!;
        }
    }
}
