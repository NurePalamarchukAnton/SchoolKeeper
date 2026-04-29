namespace WokwiSimulator.Models;

public class IncidentRequest
{
    public string DeviceGuid { get; set; } = string.Empty; // GUID устройства (MAC-адрес)
    public string IncidentType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // "Low", "Medium", "High", "Critical"
    public string? Description { get; set; }
    public DateTime? Timestamp { get; set; }
    public string Status { get; set; } = "Active"; // "Active" or "Resolved"
}

