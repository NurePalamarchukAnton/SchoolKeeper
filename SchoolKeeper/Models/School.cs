using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// ====================== School ======================
[Table("School")]
[Index(nameof(Name))]
public class School : BaseModel
{
    [Required, MaxLength(100), Column("name")] public string Name { get; set; } = default!;
    [MaxLength(255), Column("address")] public string? Address { get; set; }
    [MaxLength(100), Column("region")] public string? Region { get; set; }
    [MaxLength(20), Column("contact_number")] public string? ContactNumber { get; set; }

    // Коллекции без InverseProperty — EF сам сопоставит по навигациям
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Device> Devices { get; set; } = new List<Device>();
    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
    public ICollection<Rept> Reports { get; set; } = new List<Rept>();
}
