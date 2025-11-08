namespace SchoolKeeper.Controllers
{
    public partial class AuthController
    {
        public class AuthResponse
        {
            public string Token { get; set; } = default!;
            public string Email { get; set; } = default!;
            public string Role { get; set; } = default!;
            public int UserId { get; set; }
        }
    }
}
