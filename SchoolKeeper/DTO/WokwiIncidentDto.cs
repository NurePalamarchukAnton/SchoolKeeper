namespace SchoolKeeper.DTO;

public class WokwiIncidentDto
{
    public string DeviceGuid { get; set; } = default!; // GUID устройства (MAC-адрес)
    public string IncidentType { get; set; } = default!;
    public string Severity { get; set; } = "Low"; // "Low", "Medium", "High", "Critical"
    public string? Description { get; set; }
    public DateTime? Timestamp { get; set; }
    public string Status { get; set; } = "Active"; // "Active" or "Resolved"
}

