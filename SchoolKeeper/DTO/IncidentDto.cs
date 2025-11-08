using SchoolKeeper.Models.Enums;

namespace SchoolKeeper.DTO;

public class IncidentDto
{
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public int ReportedBy { get; set; }
    public string IncidentType { get; set; } = default!;
    public IncidentSeverity Severity { get; set; }
    public string? Description { get; set; }
    public DateTime Timestamp { get; set; }
    public IncidentStatus Status { get; set; }
    public int SchoolId { get; set; }
}

public class IncidentCreateDto
{
    public int DeviceId { get; set; }
    public int ReportedBy { get; set; }
    public string IncidentType { get; set; } = default!;
    public IncidentSeverity Severity { get; set; } = IncidentSeverity.Low;
    public string? Description { get; set; }
    public DateTime? Timestamp { get; set; }
    public IncidentStatus Status { get; set; } = IncidentStatus.Active;
    public int SchoolId { get; set; }
}

public class IncidentUpdateDto
{
    public int? DeviceId { get; set; }
    public int? ReportedBy { get; set; }
    public string? IncidentType { get; set; }
    public IncidentSeverity? Severity { get; set; }
    public string? Description { get; set; }
    public DateTime? Timestamp { get; set; }
    public IncidentStatus? Status { get; set; }
    public int? SchoolId { get; set; }
}

