using SchoolKeeper.Models.Enums;

namespace SchoolKeeper.DTO;

public class DeviceStatisticsDto
{
    public int TotalDevices { get; set; }
    public Dictionary<string, int> DevicesByType { get; set; } = new();
    public Dictionary<string, int> DevicesByStatus { get; set; } = new();
    public Dictionary<string, int> DevicesByLocation { get; set; } = new();
    public int ActiveDevices { get; set; }
    public int InactiveDevices { get; set; }
    public int ErrorDevices { get; set; }
}

public class IncidentStatisticsDto
{
    public int TotalIncidents { get; set; }
    public Dictionary<string, int> IncidentsByType { get; set; } = new();
    public Dictionary<string, int> IncidentsBySeverity { get; set; } = new();
    public Dictionary<string, int> IncidentsByStatus { get; set; } = new();
    public Dictionary<int?, int> IncidentsByDevice { get; set; } = new();
    public List<TimelineDataDto> Timeline { get; set; } = new();
    public int ActiveIncidents { get; set; }
    public int ResolvedIncidents { get; set; }
}

public class CombinedStatisticsDto
{
    public DeviceStatisticsDto DeviceStatistics { get; set; } = new();
    public IncidentStatisticsDto IncidentStatistics { get; set; } = new();
    public int SchoolId { get; set; }
    public string? SchoolName { get; set; }
    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
}

public class TimelineDataDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}

public class GroupedStatisticsDto
{
    public string Key { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class OverviewStatisticsDto
{
    public int TotalSchools { get; set; }
    public int TotalUsers { get; set; }
    public int TotalDevices { get; set; }
    public int TotalIncidents { get; set; }
    public int TotalReports { get; set; }
    public int ActiveIncidents { get; set; }
    public int ResolvedIncidents { get; set; }
}

public class SchoolStatisticsDto
{
    public int SchoolId { get; set; }
    public string? SchoolName { get; set; }
    public int TotalUsers { get; set; }
    public int TotalDevices { get; set; }
    public int TotalIncidents { get; set; }
    public int ActiveIncidents { get; set; }
    public int ResolvedIncidents { get; set; }
    public int TotalReports { get; set; }
}

public class UserStatisticsDto
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public int TotalIncidents { get; set; }
    public int ActiveIncidents { get; set; }
    public int ResolvedIncidents { get; set; }
    public int TotalReports { get; set; }
}

public class ResolutionTimeDto
{
    public double AverageResolutionTimeHours { get; set; }
    public double AverageResolutionTimeDays { get; set; }
    public int TotalResolvedIncidents { get; set; }
}

public class IncidentTrendsDto
{
    public int CurrentPeriodCount { get; set; }
    public int PreviousPeriodCount { get; set; }
    public double ChangePercentage { get; set; }
    public string Trend { get; set; } = string.Empty; // "increasing", "decreasing", "stable"
}

public class DeviceUptimeDto
{
    public int DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public double UptimePercentage { get; set; }
    public int TotalIncidents { get; set; }
    public int ErrorIncidents { get; set; }
}

public class DeviceIncidentRateDto
{
    public int DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public int TotalIncidents { get; set; }
    public double IncidentsPerDay { get; set; }
    public double IncidentsPerWeek { get; set; }
    public double IncidentsPerMonth { get; set; }
}


