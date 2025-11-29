using Entities.Models;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// ====================== Incident ======================
[Table("Incident")]
[Index(nameof(Timestamp))]
public class Incident : BaseModel
{

    [Column("device_id")] public int? DeviceId { get; set; }
    [Column("reported_by")] public int ReportedBy { get; set; }
    [Required, MaxLength(100), Column("incident_type")] public string IncidentType { get; set; } = default!;
    [NotMapped] public IncidentSeverity Severity { get => Enum.Parse<IncidentSeverity>(SeverityValue); set => SeverityValue = value.ToString(); }
    [Required, MaxLength(20), Column("severity", TypeName = "varchar(20)")] public string SeverityValue { get; private set; } = IncidentSeverity.Low.ToString();
    [Column("description", TypeName = "text")] public string? Description { get; set; }
    [Required, Column("timestamp")] public DateTime Timestamp { get; set; }
    [NotMapped] public IncidentStatus Status { get => Enum.Parse<IncidentStatus>(StatusValue); set => StatusValue = value.ToString(); }
    [Required, MaxLength(20), Column("status", TypeName = "varchar(20)")] public string StatusValue { get; private set; } = IncidentStatus.Active.ToString();
    [Column("school_id")] public int? SchoolId { get; set; }

    // FK навигации БЕЗ InverseProperty
    [ForeignKey(nameof(DeviceId))]
    [DeleteBehavior(DeleteBehavior.SetNull)]   // При удалении устройства устанавливаем DeviceId в NULL, сохраняя историю инцидентов
    public Device? Device { get; set; }

    [ForeignKey(nameof(ReportedBy))]
    [DeleteBehavior(DeleteBehavior.Restrict)]   // важно
    public User Reporter { get; set; } = default!;

    [ForeignKey(nameof(SchoolId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]   // важно
    public School? School { get; set; }

    public ICollection<ReptIncident> ReptIncidents { get; set; } = new List<ReptIncident>();
    public ICollection<UserIncident> UserIncidents { get; set; } = new List<UserIncident>();
}
