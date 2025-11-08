using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

// ====================== UserIncident (junction) ======================
[Table("UserIncident")]
[PrimaryKey(nameof(UserId), nameof(IncidentId))]
public class UserIncident : BaseModel
{
    [Column("user_id")] public int UserId { get; set; }
    [Column("incident_id")] public int IncidentId { get; set; }

    [ForeignKey(nameof(UserId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public User User { get; set; } = default!;

    [ForeignKey(nameof(IncidentId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Incident Incident { get; set; } = default!;
}
