using SchoolKeeper.Models.Enums;

namespace SchoolKeeper.DTO;

public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = default!;
    public UserRole Role { get; set; }
    public string RoleString => Role.ToString();
    public string Email { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public int SchoolId { get; set; }
}

public class UserCreateDto
{
    public string FullName { get; set; } = default!;
    public UserRole Role { get; set; } = UserRole.Student;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public int SchoolId { get; set; }
}

public class UserUpdateDto
{
    public string? FullName { get; set; }
    public UserRole? Role { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? PhoneNumber { get; set; }
    public int? SchoolId { get; set; }
}

