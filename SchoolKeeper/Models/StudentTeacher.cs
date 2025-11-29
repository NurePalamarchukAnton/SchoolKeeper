using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

// ====================== StudentTeacher (junction) ======================
[Table("StudentTeacher")]
[PrimaryKey(nameof(StudentId), nameof(TeacherId))]
public class StudentTeacher : BaseModel
{
    [Column("student_id")] public int StudentId { get; set; }
    [Column("teacher_id")] public int TeacherId { get; set; }

    [ForeignKey(nameof(StudentId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    [InverseProperty(nameof(User.StudentTeacherRelationships))]
    public User Student { get; set; } = default!;

    [ForeignKey(nameof(TeacherId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    [InverseProperty(nameof(User.TeacherStudentRelationships))]
    public User Teacher { get; set; } = default!;
}

