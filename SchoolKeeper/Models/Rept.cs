using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// ====================== Rept (Report) ======================
[Table("Rept")]
public class Rept : BaseModel
{

    [Column("school_id")] public int SchoolId { get; set; }
    [Column("generated_by")] public int GeneratedBy { get; set; }

    [Required, Column("period_start", TypeName = "date")] public DateOnly PeriodStart { get; set; }
    [Required, Column("period_end", TypeName = "date")] public DateOnly PeriodEnd { get; set; }
    [Column("summary", TypeName = "text")] public string? Summary { get; set; }
    [Required, Column("generated_on")] public DateTime GeneratedOn { get; set; }

    [ForeignKey(nameof(SchoolId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public School School { get; set; } = default!;

    [ForeignKey(nameof(GeneratedBy))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public User Generator { get; set; } = default!;

    public ICollection<ReptIncident> ReptIncidents { get; set; } = new List<ReptIncident>();
}
