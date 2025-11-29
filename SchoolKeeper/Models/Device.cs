using Entities.Models;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// ====================== Device ======================
[Table("Device")]
public class Device : BaseModel
{
    [Required, MaxLength(100), Column("device_name")] public string DeviceName { get; set; } = default!;

    [NotMapped] public DeviceType DeviceType { get => Enum.Parse<DeviceType>(DeviceTypeValue); set => DeviceTypeValue = value.ToString(); }
    [Required, MaxLength(30), Column("device_type", TypeName = "varchar(30)")] public string DeviceTypeValue { get; private set; } = DeviceType.MotionSensor.ToString();

    [NotMapped] public DeviceStatus Status { get => Enum.Parse<DeviceStatus>(StatusValue); set => StatusValue = value.ToString(); }
    [Required, MaxLength(20), Column("status", TypeName = "varchar(20)")] public string StatusValue { get; private set; } = DeviceStatus.Active.ToString();

    [MaxLength(100), Column("location")] public string? Location { get; set; }

    [MaxLength(36), Column("device_guid", TypeName = "varchar(36)")]
    public string? DeviceGuid { get; set; } // GUID устройства (MAC-адрес для WOKWI)

    [Column("school_id")] public int? SchoolId { get; set; }

    [ForeignKey(nameof(SchoolId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public School? School { get; set; }

    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
}
