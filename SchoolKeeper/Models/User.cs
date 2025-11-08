using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// ====================== User ======================
[Table("User")]
[Index(nameof(Email), IsUnique = true)]
public class User : BaseModel
{
    [Required, MaxLength(100), Column("full_name")] public string FullName { get; set; } = default!;

    [NotMapped] public UserRole Role { get => Enum.Parse<UserRole>(RoleValue); set => RoleValue = value.ToString(); }
    [Required, MaxLength(20), Column("role", TypeName = "varchar(20)")] public string RoleValue { get; private set; } = UserRole.Student.ToString();

    [Required, MaxLength(100), Column("email")] public string Email { get; set; } = default!;
    [Required, MaxLength(255), Column("password_hash")] public string PasswordHash { get; set; } = default!;
    [MaxLength(20), Column("phone_number")] public string? PhoneNumber { get; set; }

    [Column("school_id")] public int SchoolId { get; set; }

    [ForeignKey(nameof(SchoolId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public School School { get; set; } = default!;

    public ICollection<Incident> ReportedIncidents { get; set; } = new List<Incident>();
    public ICollection<UserIncident> UserIncidents { get; set; } = new List<UserIncident>();
    public ICollection<Rept> GeneratedReports { get; set; } = new List<Rept>();
}
