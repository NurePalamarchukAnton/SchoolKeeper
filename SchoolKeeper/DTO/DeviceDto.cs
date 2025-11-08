using SchoolKeeper.Models.Enums;

namespace SchoolKeeper.DTO;

public class DeviceDto
{
    public int Id { get; set; }
    public string DeviceName { get; set; } = default!;
    public DeviceType DeviceType { get; set; }
    public DeviceStatus Status { get; set; }
    public string? Location { get; set; }
    public int SchoolId { get; set; }
}

public class DeviceCreateDto
{
    public string DeviceName { get; set; } = default!;
    public DeviceType DeviceType { get; set; }
    public DeviceStatus Status { get; set; } = DeviceStatus.Active;
    public string? Location { get; set; }
    public int SchoolId { get; set; }
}

public class DeviceUpdateDto
{
    public string? DeviceName { get; set; }
    public DeviceType? DeviceType { get; set; }
    public DeviceStatus? Status { get; set; }
    public string? Location { get; set; }
    public int? SchoolId { get; set; }
}

