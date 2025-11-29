namespace SchoolKeeper.DTO;

public class AuthResponse
{
    public string Token { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Role { get; set; } = default!;
    public int UserId { get; set; }
    public string? OriginalAdminId { get; set; }
}
