using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

// ====================== ParentStudent (junction) ======================
[Table("ParentStudent")]
[PrimaryKey(nameof(ParentId), nameof(StudentId))]
public class ParentStudent : BaseModel
{
    [Column("parent_id")] public int ParentId { get; set; }
    [Column("student_id")] public int StudentId { get; set; }

    [ForeignKey(nameof(ParentId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    [InverseProperty(nameof(User.ParentRelationships))]
    public User Parent { get; set; } = default!;

    [ForeignKey(nameof(StudentId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    [InverseProperty(nameof(User.StudentRelationships))]
    public User Student { get; set; } = default!;
}

