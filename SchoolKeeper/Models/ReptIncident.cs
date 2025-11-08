using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
// ====================== ReptIncident (junction) ======================
[Table("ReptIncident")]
[PrimaryKey(nameof(ReptId), nameof(IncidentId))]
public class ReptIncident : BaseModel
{
    [Column("rept_id")] public int ReptId { get; set; }
    [Column("incident_id")] public int IncidentId { get; set; }

    [ForeignKey(nameof(ReptId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Rept Rept { get; set; } = default!;

    [ForeignKey(nameof(IncidentId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Incident Incident { get; set; } = default!;
}
