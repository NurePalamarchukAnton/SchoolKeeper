using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.Authorization;
using SchoolKeeper.DTO;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Response;
using SchoolKeeper.Services;

namespace SchoolKeeper.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatisticsController : ControllerBase
{
    private readonly SchoolKeeperDbContext _context;
    private readonly IUserService _userService;

    public StatisticsController(SchoolKeeperDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    // ========== Отчетности по устройствам ==========

    [HttpGet("devices/report")]
    public async Task<ActionResult<ResponseWrapper<DeviceStatisticsDto>>> GetDevicesReport(
        [FromQuery] int? schoolId,
        [FromQuery] string? deviceType,
        [FromQuery] string? status,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null) throw new UnauthorizedException();

        // Согласно матрице: Admin, Security
        if (user.Role != UserRole.Admin && user.Role != UserRole.Security)
        {
            throw new UnauthorizedException("You don't have access to device statistics.");
        }

        var query = _context.Devices.AsQueryable();

        // Filter by school
        if (schoolId.HasValue)
        {
            query = query.Where(d => d.SchoolId == schoolId.Value);
        }
        else if (user.Role != UserRole.Admin)
        {
            query = query.Where(d => d.SchoolId == user.SchoolId);
        }

        // Filter by device type
        if (!string.IsNullOrEmpty(deviceType))
        {
            query = query.Where(d => d.DeviceTypeValue == deviceType);
        }

        // Filter by status
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(d => d.StatusValue == status);
        }

        var devices = await query.ToListAsync();

        var stats = new DeviceStatisticsDto
        {
            TotalDevices = devices.Count,
            DevicesByType = devices.GroupBy(d => d.DeviceTypeValue)
                .ToDictionary(g => g.Key, g => g.Count()),
            DevicesByStatus = devices.GroupBy(d => d.StatusValue)
                .ToDictionary(g => g.Key, g => g.Count()),
            DevicesByLocation = devices.Where(d => !string.IsNullOrEmpty(d.Location))
                .GroupBy(d => d.Location!)
                .ToDictionary(g => g.Key, g => g.Count()),
            ActiveDevices = devices.Count(d => d.StatusValue == DeviceStatus.Active.ToString()),
            InactiveDevices = devices.Count(d => d.StatusValue == DeviceStatus.Inactive.ToString()),
            ErrorDevices = devices.Count(d => d.StatusValue == DeviceStatus.Error.ToString())
        };

        var response = new ResponseWrapper<DeviceStatisticsDto>(200, stats);
        return Ok(response);
    }

    [HttpGet("devices/by-type")]
    public async Task<ActionResult<ResponseWrapper<Dictionary<string, int>>>> GetDevicesByType([FromQuery] int? schoolId)
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null) throw new UnauthorizedException();

        // Согласно матрице: Admin, Security
        if (user.Role != UserRole.Admin && user.Role != UserRole.Security)
        {
            throw new UnauthorizedException("You don't have access to device statistics.");
        }

        var query = _context.Devices.AsQueryable();

        if (schoolId.HasValue)
        {
            query = query.Where(d => d.SchoolId == schoolId.Value);
        }
        else if (user.Role != UserRole.Admin)
        {
            query = query.Where(d => d.SchoolId == user.SchoolId);
        }

        var result = await query
            .GroupBy(d => d.DeviceTypeValue)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        var response = new ResponseWrapper<Dictionary<string, int>>(200, result);
        return Ok(response);
    }

    [HttpGet("devices/by-status")]
    public async Task<ActionResult<ResponseWrapper<Dictionary<string, int>>>> GetDevicesByStatus([FromQuery] int? schoolId)
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null) throw new UnauthorizedException();

        // Согласно матрице: Admin, Security
        if (user.Role != UserRole.Admin && user.Role != UserRole.Security)
        {
            throw new UnauthorizedException("You don't have access to device statistics.");
        }

        var query = _context.Devices.AsQueryable();

        if (schoolId.HasValue)
        {
            query = query.Where(d => d.SchoolId == schoolId.Value);
        }
        else if (user.Role != UserRole.Admin)
        {
            query = query.Where(d => d.SchoolId == user.SchoolId);
        }

        var result = await query
            .GroupBy(d => d.StatusValue)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        var response = new ResponseWrapper<Dictionary<string, int>>(200, result);
        return Ok(response);
    }

    [HttpGet("devices/by-location")]
    public async Task<ActionResult<ResponseWrapper<Dictionary<string, int>>>> GetDevicesByLocation([FromQuery] int? schoolId)
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null) throw new UnauthorizedException();

        // Согласно матрице: Admin, Security
        if (user.Role != UserRole.Admin && user.Role != UserRole.Security)
        {
            throw new UnauthorizedException("You don't have access to device statistics.");
        }

        var query = _context.Devices.Where(d => !string.IsNullOrEmpty(d.Location)).AsQueryable();

        if (schoolId.HasValue)
        {
            query = query.Where(d => d.SchoolId == schoolId.Value);
        }
        else if (user.Role != UserRole.Admin)
        {
            query = query.Where(d => d.SchoolId == user.SchoolId);
        }

        var result = await query
            .GroupBy(d => d.Location!)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        var response = new ResponseWrapper<Dictionary<string, int>>(200, result);
        return Ok(response);
    }

    // ========== Отчетности по инцидентам ==========

    [HttpGet("incidents/report")]
    public async Task<ActionResult<ResponseWrapper<IncidentStatisticsDto>>> GetIncidentsReport(
        [FromQuery] int? schoolId,
        [FromQuery] int? deviceId,
        [FromQuery] string? severity,
        [FromQuery] string? status,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null) throw new UnauthorizedException();

        // Согласно матрице: Admin, Security, Teacher, Parent, Student
        if (user.Role != UserRole.Admin && user.Role != UserRole.Security && 
            user.Role != UserRole.Teacher && user.Role != UserRole.Parent && 
            user.Role != UserRole.Student)
        {
            throw new UnauthorizedException("You don't have access to incident statistics.");
        }

        var query = _context.Incidents.AsQueryable();

        // Filter by school
        if (schoolId.HasValue)
        {
            query = query.Where(i => i.SchoolId == schoolId.Value);
        }
        else if (user.Role != UserRole.Admin)
        {
            query = query.Where(i => i.SchoolId == user.SchoolId);
        }

        // Student и Teacher видят только свои инциденты
        if (user.Role == UserRole.Student || user.Role == UserRole.Teacher)
        {
            query = query.Where(i => i.ReportedBy == user.Id);
        }

        // Filter by device
        if (deviceId.HasValue)
        {
            query = query.Where(i => i.DeviceId == deviceId.Value);
        }

        // Filter by severity
        if (!string.IsNullOrEmpty(severity))
        {
            query = query.Where(i => i.SeverityValue == severity);
        }

        // Filter by status
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(i => i.StatusValue == status);
        }

        // Filter by date range
        if (startDate.HasValue)
        {
            query = query.Where(i => i.Timestamp >= startDate.Value);
        }
        if (endDate.HasValue)
        {
            query = query.Where(i => i.Timestamp <= endDate.Value);
        }

        var incidents = await query.ToListAsync();

        var timeline = incidents
            .GroupBy(i => i.Timestamp.Date)
            .Select(g => new TimelineDataDto
            {
                Date = g.Key,
                Count = g.Count()
            })
            .OrderBy(t => t.Date)
            .ToList();

        var stats = new IncidentStatisticsDto
        {
            TotalIncidents = incidents.Count,
            IncidentsByType = incidents.GroupBy(i => i.IncidentType)
                .ToDictionary(g => g.Key, g => g.Count()),
            IncidentsBySeverity = incidents.GroupBy(i => i.SeverityValue)
                .ToDictionary(g => g.Key, g => g.Count()),
            IncidentsByStatus = incidents.GroupBy(i => i.StatusValue)
                .ToDictionary(g => g.Key, g => g.Count()),
            IncidentsByDevice = incidents
                .Where(i => i.DeviceId.HasValue)
                .GroupBy(i => i.DeviceId!.Value)
                .ToDictionary(g => (int?)g.Key, g => g.Count()),
            Timeline = timeline,
            ActiveIncidents = incidents.Count(i => i.StatusValue == IncidentStatus.Active.ToString()),
            ResolvedIncidents = incidents.Count(i => i.StatusValue == IncidentStatus.Resolved.ToString())
        };

        var response = new ResponseWrapper<IncidentStatisticsDto>(200, stats);
        return Ok(response);
    }

    [HttpGet("incidents/by-type")]
    public async Task<ActionResult<ResponseWrapper<Dictionary<string, int>>>> GetIncidentsByType([FromQuery] int? schoolId)
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null) throw new UnauthorizedException();

        // Согласно матрице: Admin, Security, Teacher, Parent, Student
        if (user.Role != UserRole.Admin && user.Role != UserRole.Security && 
            user.Role != UserRole.Teacher && user.Role != UserRole.Parent && 
            user.Role != UserRole.Student)
        {
            throw new UnauthorizedException("You don't have access to incident statistics.");
        }

        var query = _context.Incidents.AsQueryable();

        if (schoolId.HasValue)
        {
            query = query.Where(i => i.SchoolId == schoolId.Value);
        }
        else if (user.Role != UserRole.Admin)
        {
            query = query.Where(i => i.SchoolId == user.SchoolId);
        }

        if (user.Role == UserRole.Student || user.Role == UserRole.Teacher)
        {
            query = query.Where(i => i.ReportedBy == user.Id);
        }

        var result = await query
            .GroupBy(i => i.IncidentType)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        var response = new ResponseWrapper<Dictionary<string, int>>(200, result);
        return Ok(response);
    }

    [HttpGet("incidents/by-severity")]
    public async Task<ActionResult<ResponseWrapper<Dictionary<string, int>>>> GetIncidentsBySeverity([FromQuery] int? schoolId)
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null) throw new UnauthorizedException();

        // Согласно матрице: Admin, Security, Teacher, Parent, Student
        if (user.Role != UserRole.Admin && user.Role != UserRole.Security && 
            user.Role != UserRole.Teacher && user.Role != UserRole.Parent && 
            user.Role != UserRole.Student)
        {
            throw new UnauthorizedException("You don't have access to incident statistics.");
        }

        var query = _context.Incidents.AsQueryable();

        if (schoolId.HasValue)
        {
            query = query.Where(i => i.SchoolId == schoolId.Value);
        }
        else if (user.Role != UserRole.Admin)
        {
            query = query.Where(i => i.SchoolId == user.SchoolId);
        }

        if (user.Role == UserRole.Student || user.Role == UserRole.Teacher)
        {
            query = query.Where(i => i.ReportedBy == user.Id);
        }

        var result = await query
            .GroupBy(i => i.SeverityValue)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        var response = new ResponseWrapper<Dictionary<string, int>>(200, result);
        return Ok(response);
    }

    [HttpGet("incidents/by-status")]
    public async Task<ActionResult<ResponseWrapper<Dictionary<string, int>>>> GetIncidentsByStatus([FromQuery] int? schoolId)
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null) throw new UnauthorizedException();

        // Согласно матрице: Admin, Security, Teacher, Parent, Student
        if (user.Role != UserRole.Admin && user.Role != UserRole.Security && 
            user.Role != UserRole.Teacher && user.Role != UserRole.Parent && 
            user.Role != UserRole.Student)
        {
            throw new UnauthorizedException("You don't have access to incident statistics.");
        }

        var query = _context.Incidents.AsQueryable();

        if (schoolId.HasValue)
        {
            query = query.Where(i => i.SchoolId == schoolId.Value);
        }
        else if (user.Role != UserRole.Admin)
        {
            query = query.Where(i => i.SchoolId == user.SchoolId);
        }

        if (user.Role == UserRole.Student || user.Role == UserRole.Teacher)
        {
            query = query.Where(i => i.ReportedBy == user.Id);
        }

        var result = await query
            .GroupBy(i => i.StatusValue)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        var response = new ResponseWrapper<Dictionary<string, int>>(200, result);
        return Ok(response);
    }

    [HttpGet("incidents/timeline")]
    public async Task<ActionResult<ResponseWrapper<List<TimelineDataDto>>>> GetIncidentsTimeline(
        [FromQuery] int? schoolId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null) throw new UnauthorizedException();

        // Согласно матрице: Admin, Security, Teacher, Parent, Student
        if (user.Role != UserRole.Admin && user.Role != UserRole.Security && 
            user.Role != UserRole.Teacher && user.Role != UserRole.Parent && 
            user.Role != UserRole.Student)
        {
            throw new UnauthorizedException("You don't have access to incident statistics.");
        }

        var query = _context.Incidents.AsQueryable();

        if (schoolId.HasValue)
        {
            query = query.Where(i => i.SchoolId == schoolId.Value);
        }
        else if (user.Role != UserRole.Admin)
        {
            query = query.Where(i => i.SchoolId == user.SchoolId);
        }

        if (user.Role == UserRole.Student || user.Role == UserRole.Teacher)
        {
            query = query.Where(i => i.ReportedBy == user.Id);
        }

        if (startDate.HasValue)
        {
            query = query.Where(i => i.Timestamp >= startDate.Value);
        }
        if (endDate.HasValue)
        {
            query = query.Where(i => i.Timestamp <= endDate.Value);
        }

        var timeline = await query
            .GroupBy(i => i.Timestamp.Date)
            .Select(g => new TimelineDataDto
            {
                Date = g.Key,
                Count = g.Count()
            })
            .OrderBy(t => t.Date)
            .ToListAsync();

        var response = new ResponseWrapper<List<TimelineDataDto>>(200, timeline);
        return Ok(response);
    }

    [HttpGet("incidents/by-device")]
    public async Task<ActionResult<ResponseWrapper<Dictionary<int, int>>>> GetIncidentsByDevice([FromQuery] int? schoolId)
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null) throw new UnauthorizedException();

        // Согласно матрице: Admin, Security, Teacher, Parent, Student
        if (user.Role != UserRole.Admin && user.Role != UserRole.Security && 
            user.Role != UserRole.Teacher && user.Role != UserRole.Parent && 
            user.Role != UserRole.Student)
        {
            throw new UnauthorizedException("You don't have access to incident statistics.");
        }

        var query = _context.Incidents.AsQueryable();

        if (schoolId.HasValue)
        {
            query = query.Where(i => i.SchoolId == schoolId.Value);
        }
        else if (user.Role != UserRole.Admin)
        {
            query = query.Where(i => i.SchoolId == user.SchoolId);
        }

        if (user.Role == UserRole.Student || user.Role == UserRole.Teacher)
        {
            query = query.Where(i => i.ReportedBy == user.Id);
        }

        var result = await query
            .Where(i => i.DeviceId.HasValue)
            .GroupBy(i => i.DeviceId!.Value)
            .ToDictionaryAsync(g => (int?)g.Key, g => g.Count());

        var response = new ResponseWrapper<Dictionary<int?, int>>(200, result);
        return Ok(response);
    }

    // ========== Комбинированные отчеты ==========

    [HttpGet("combined/report")]
    public async Task<ActionResult<ResponseWrapper<CombinedStatisticsDto>>> GetCombinedReport(
        [FromQuery] int? schoolId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null) throw new UnauthorizedException();

        // Согласно матрице: Admin, Security, Teacher
        if (user.Role != UserRole.Admin && user.Role != UserRole.Security && user.Role != UserRole.Teacher)
        {
            throw new UnauthorizedException("You don't have access to combined statistics.");
        }

        var targetSchoolId = schoolId ?? (user.Role == UserRole.Admin ? (int?)null : user.SchoolId);

        // Get device statistics
        var devicesQuery = _context.Devices.AsQueryable();
        if (targetSchoolId.HasValue)
        {
            devicesQuery = devicesQuery.Where(d => d.SchoolId == targetSchoolId.Value);
        }
        var devices = await devicesQuery.ToListAsync();

        var deviceStats = new DeviceStatisticsDto
        {
            TotalDevices = devices.Count,
            DevicesByType = devices.GroupBy(d => d.DeviceTypeValue)
                .ToDictionary(g => g.Key, g => g.Count()),
            DevicesByStatus = devices.GroupBy(d => d.StatusValue)
                .ToDictionary(g => g.Key, g => g.Count()),
            DevicesByLocation = devices.Where(d => !string.IsNullOrEmpty(d.Location))
                .GroupBy(d => d.Location!)
                .ToDictionary(g => g.Key, g => g.Count()),
            ActiveDevices = devices.Count(d => d.StatusValue == DeviceStatus.Active.ToString()),
            InactiveDevices = devices.Count(d => d.StatusValue == DeviceStatus.Inactive.ToString()),
            ErrorDevices = devices.Count(d => d.StatusValue == DeviceStatus.Error.ToString())
        };

        // Get incident statistics
        var incidentsQuery = _context.Incidents.AsQueryable();
        if (targetSchoolId.HasValue)
        {
            incidentsQuery = incidentsQuery.Where(i => i.SchoolId == targetSchoolId.Value);
        }
        if (startDate.HasValue)
        {
            incidentsQuery = incidentsQuery.Where(i => i.Timestamp >= startDate.Value);
        }
        if (endDate.HasValue)
        {
            incidentsQuery = incidentsQuery.Where(i => i.Timestamp <= endDate.Value);
        }
        var incidents = await incidentsQuery.ToListAsync();

        var timeline = incidents
            .GroupBy(i => i.Timestamp.Date)
            .Select(g => new TimelineDataDto
            {
                Date = g.Key,
                Count = g.Count()
            })
            .OrderBy(t => t.Date)
            .ToList();

        var incidentStats = new IncidentStatisticsDto
        {
            TotalIncidents = incidents.Count,
            IncidentsByType = incidents.GroupBy(i => i.IncidentType)
                .ToDictionary(g => g.Key, g => g.Count()),
            IncidentsBySeverity = incidents.GroupBy(i => i.SeverityValue)
                .ToDictionary(g => g.Key, g => g.Count()),
            IncidentsByStatus = incidents.GroupBy(i => i.StatusValue)
                .ToDictionary(g => g.Key, g => g.Count()),
            IncidentsByDevice = incidents
                .Where(i => i.DeviceId.HasValue)
                .GroupBy(i => i.DeviceId!.Value)
                .ToDictionary(g => (int?)g.Key, g => g.Count()),
            Timeline = timeline,
            ActiveIncidents = incidents.Count(i => i.StatusValue == IncidentStatus.Active.ToString()),
            ResolvedIncidents = incidents.Count(i => i.StatusValue == IncidentStatus.Resolved.ToString())
        };

        var school = targetSchoolId.HasValue 
            ? await _context.Schools.FirstOrDefaultAsync(s => s.Id == targetSchoolId.Value)
            : null;

        var combined = new CombinedStatisticsDto
        {
            DeviceStatistics = deviceStats,
            IncidentStatistics = incidentStats,
            SchoolId = targetSchoolId ?? 0,
            SchoolName = school?.Name,
            PeriodStart = startDate.HasValue ? DateOnly.FromDateTime(startDate.Value) : null,
            PeriodEnd = endDate.HasValue ? DateOnly.FromDateTime(endDate.Value) : null
        };

        var response = new ResponseWrapper<CombinedStatisticsDto>(200, combined);
        return Ok(response);
    }

    // ========== Общая статистика ==========

    [HttpGet("overview")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<ActionResult<ResponseWrapper<OverviewStatisticsDto>>> GetOverview()
    {
        var stats = new OverviewStatisticsDto
        {
            TotalSchools = await _context.Schools.CountAsync(),
            TotalUsers = await _context.Users.CountAsync(),
            TotalDevices = await _context.Devices.CountAsync(),
            TotalIncidents = await _context.Incidents.CountAsync(),
            TotalReports = await _context.Reports.CountAsync(),
            ActiveIncidents = await _context.Incidents
                .CountAsync(i => i.StatusValue == IncidentStatus.Active.ToString()),
            ResolvedIncidents = await _context.Incidents
                .CountAsync(i => i.StatusValue == IncidentStatus.Resolved.ToString())
        };

        var response = new ResponseWrapper<OverviewStatisticsDto>(200, stats);
        return Ok(response);
    }

    [HttpGet("school/{schoolId:int}")]
    public async Task<ActionResult<ResponseWrapper<SchoolStatisticsDto>>> GetSchoolStatistics(int schoolId)
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null) throw new UnauthorizedException();

        // Согласно матрице: Admin, Security (для своей школы)
        if (user.Role != UserRole.Admin && user.Role != UserRole.Security)
        {
            throw new UnauthorizedException("You don't have access to school statistics.");
        }

        if (user.Role != UserRole.Admin && user.SchoolId != schoolId)
        {
            throw new UnauthorizedException("You don't have access to this school's statistics.");
        }

        var school = await _context.Schools.FirstOrDefaultAsync(s => s.Id == schoolId);
        if (school == null) throw new NotFoundException();

        var stats = new SchoolStatisticsDto
        {
            SchoolId = schoolId,
            SchoolName = school.Name,
            TotalUsers = await _context.Users.CountAsync(u => u.SchoolId == schoolId),
            TotalDevices = await _context.Devices.CountAsync(d => d.SchoolId == schoolId),
            TotalIncidents = await _context.Incidents.CountAsync(i => i.SchoolId == schoolId),
            ActiveIncidents = await _context.Incidents
                .CountAsync(i => i.SchoolId == schoolId && i.StatusValue == IncidentStatus.Active.ToString()),
            ResolvedIncidents = await _context.Incidents
                .CountAsync(i => i.SchoolId == schoolId && i.StatusValue == IncidentStatus.Resolved.ToString()),
            TotalReports = await _context.Reports.CountAsync(r => r.SchoolId == schoolId)
        };

        var response = new ResponseWrapper<SchoolStatisticsDto>(200, stats);
        return Ok(response);
    }

    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<ResponseWrapper<UserStatisticsDto>>> GetUserStatistics(int userId)
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null) throw new UnauthorizedException();

        // Согласно матрице: Admin, Security (для своей школы), сам пользователь
        if (user.Role != UserRole.Admin && user.Role != UserRole.Security && user.Id != userId)
        {
            throw new UnauthorizedException("You don't have access to user statistics.");
        }

        var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (targetUser == null) throw new NotFoundException();

        if (user.Role == UserRole.Security && targetUser.SchoolId != user.SchoolId)
        {
            throw new UnauthorizedException("You don't have access to this user's statistics.");
        }

        var stats = new UserStatisticsDto
        {
            UserId = userId,
            UserName = targetUser.FullName,
            TotalIncidents = await _context.Incidents.CountAsync(i => i.ReportedBy == userId),
            ActiveIncidents = await _context.Incidents
                .CountAsync(i => i.ReportedBy == userId && i.StatusValue == IncidentStatus.Active.ToString()),
            ResolvedIncidents = await _context.Incidents
                .CountAsync(i => i.ReportedBy == userId && i.StatusValue == IncidentStatus.Resolved.ToString()),
            TotalReports = await _context.Reports.CountAsync(r => r.GeneratedBy == userId)
        };

        var response = new ResponseWrapper<UserStatisticsDto>(200, stats);
        return Ok(response);
    }

    // ========== Аналитика ==========

    [HttpGet("incidents/resolution-time")]
    public async Task<ActionResult<ResponseWrapper<ResolutionTimeDto>>> GetResolutionTime([FromQuery] int? schoolId)
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null) throw new UnauthorizedException();

        // Согласно матрице: Admin, Security, Teacher
        if (user.Role != UserRole.Admin && user.Role != UserRole.Security && user.Role != UserRole.Teacher)
        {
            throw new UnauthorizedException("You don't have access to resolution time statistics.");
        }

        var query = _context.Incidents
            .Where(i => i.StatusValue == IncidentStatus.Resolved.ToString())
            .AsQueryable();

        if (schoolId.HasValue)
        {
            query = query.Where(i => i.SchoolId == schoolId.Value);
        }
        else if (user.Role != UserRole.Admin)
        {
            query = query.Where(i => i.SchoolId == user.SchoolId);
        }

        var resolvedIncidents = await query.ToListAsync();

        if (!resolvedIncidents.Any())
        {
            var emptyStats = new ResolutionTimeDto
            {
                AverageResolutionTimeHours = 0,
                AverageResolutionTimeDays = 0,
                TotalResolvedIncidents = 0
            };
            var emptyResponse = new ResponseWrapper<ResolutionTimeDto>(200, emptyStats);
            return Ok(emptyResponse);
        }

        // Calculate average resolution time
        // Note: We need to track when incidents were resolved. For now, using Timestamp as creation time
        // In a real system, you'd have a ResolvedAt field
        var totalHours = resolvedIncidents.Sum(i => (DateTime.UtcNow - i.Timestamp).TotalHours);
        var avgHours = totalHours / resolvedIncidents.Count;
        var avgDays = avgHours / 24;

        var stats = new ResolutionTimeDto
        {
            AverageResolutionTimeHours = Math.Round(avgHours, 2),
            AverageResolutionTimeDays = Math.Round(avgDays, 2),
            TotalResolvedIncidents = resolvedIncidents.Count
        };

        var response = new ResponseWrapper<ResolutionTimeDto>(200, stats);
        return Ok(response);
    }

    [HttpGet("incidents/trends")]
    public async Task<ActionResult<ResponseWrapper<IncidentTrendsDto>>> GetIncidentTrends([FromQuery] int? schoolId)
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null) throw new UnauthorizedException();

        // Согласно матрице: Admin, Security, Teacher
        if (user.Role != UserRole.Admin && user.Role != UserRole.Security && user.Role != UserRole.Teacher)
        {
            throw new UnauthorizedException("You don't have access to incident trends.");
        }

        var query = _context.Incidents.AsQueryable();

        if (schoolId.HasValue)
        {
            query = query.Where(i => i.SchoolId == schoolId.Value);
        }
        else if (user.Role != UserRole.Admin)
        {
            query = query.Where(i => i.SchoolId == user.SchoolId);
        }

        var now = DateTime.UtcNow;
        var currentPeriodStart = now.AddDays(-30);
        var previousPeriodStart = currentPeriodStart.AddDays(-30);
        var previousPeriodEnd = currentPeriodStart;

        var currentPeriodCount = await query
            .CountAsync(i => i.Timestamp >= currentPeriodStart && i.Timestamp <= now);

        var previousPeriodCount = await query
            .CountAsync(i => i.Timestamp >= previousPeriodStart && i.Timestamp < previousPeriodEnd);

        var changePercentage = previousPeriodCount > 0
            ? ((currentPeriodCount - previousPeriodCount) / (double)previousPeriodCount) * 100
            : (currentPeriodCount > 0 ? 100 : 0);

        var trend = changePercentage > 5 ? "increasing" : changePercentage < -5 ? "decreasing" : "stable";

        var stats = new IncidentTrendsDto
        {
            CurrentPeriodCount = currentPeriodCount,
            PreviousPeriodCount = previousPeriodCount,
            ChangePercentage = Math.Round(changePercentage, 2),
            Trend = trend
        };

        var response = new ResponseWrapper<IncidentTrendsDto>(200, stats);
        return Ok(response);
    }

    [HttpGet("devices/uptime")]
    public async Task<ActionResult<ResponseWrapper<List<DeviceUptimeDto>>>> GetDeviceUptime([FromQuery] int? schoolId)
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null) throw new UnauthorizedException();

        // Согласно матрице: Admin, Security
        if (user.Role != UserRole.Admin && user.Role != UserRole.Security)
        {
            throw new UnauthorizedException("You don't have access to device uptime statistics.");
        }

        var devicesQuery = _context.Devices.Include(d => d.Incidents).AsQueryable();

        if (schoolId.HasValue)
        {
            devicesQuery = devicesQuery.Where(d => d.SchoolId == schoolId.Value);
        }
        else if (user.Role != UserRole.Admin)
        {
            devicesQuery = devicesQuery.Where(d => d.SchoolId == user.SchoolId);
        }

        var devices = await devicesQuery.ToListAsync();

        var result = devices.Select(d =>
        {
            var totalIncidents = d.Incidents.Count;
            var errorIncidents = d.Incidents.Count(i => i.SeverityValue == IncidentSeverity.Critical.ToString() || 
                                                       i.SeverityValue == IncidentSeverity.High.ToString());
            var uptimePercentage = totalIncidents > 0 
                ? Math.Max(0, 100 - (errorIncidents * 100.0 / totalIncidents))
                : 100;

            return new DeviceUptimeDto
            {
                DeviceId = d.Id,
                DeviceName = d.DeviceName,
                UptimePercentage = Math.Round(uptimePercentage, 2),
                TotalIncidents = totalIncidents,
                ErrorIncidents = errorIncidents
            };
        }).ToList();

        var response = new ResponseWrapper<List<DeviceUptimeDto>>(200, result);
        return Ok(response);
    }

    [HttpGet("devices/incident-rate")]
    public async Task<ActionResult<ResponseWrapper<List<DeviceIncidentRateDto>>>> GetDeviceIncidentRate([FromQuery] int? schoolId)
    {
        var user = await _userService.GetCurrentUserAsync(HttpContext);
        if (user == null) throw new UnauthorizedException();

        // Согласно матрице: Admin, Security
        if (user.Role != UserRole.Admin && user.Role != UserRole.Security)
        {
            throw new UnauthorizedException("You don't have access to device incident rate statistics.");
        }

        var devicesQuery = _context.Devices.Include(d => d.Incidents).AsQueryable();

        if (schoolId.HasValue)
        {
            devicesQuery = devicesQuery.Where(d => d.SchoolId == schoolId.Value);
        }
        else if (user.Role != UserRole.Admin)
        {
            devicesQuery = devicesQuery.Where(d => d.SchoolId == user.SchoolId);
        }

        var devices = await devicesQuery.ToListAsync();

        var result = devices.Select(d =>
        {
            var totalIncidents = d.Incidents.Count;
            var oldestIncident = d.Incidents.OrderBy(i => i.Timestamp).FirstOrDefault();
            var daysSinceFirstIncident = oldestIncident != null 
                ? (DateTime.UtcNow - oldestIncident.Timestamp).TotalDays 
                : 1;

            var incidentsPerDay = daysSinceFirstIncident > 0 
                ? totalIncidents / daysSinceFirstIncident 
                : 0;
            var incidentsPerWeek = incidentsPerDay * 7;
            var incidentsPerMonth = incidentsPerDay * 30;

            return new DeviceIncidentRateDto
            {
                DeviceId = d.Id,
                DeviceName = d.DeviceName,
                TotalIncidents = totalIncidents,
                IncidentsPerDay = Math.Round(incidentsPerDay, 2),
                IncidentsPerWeek = Math.Round(incidentsPerWeek, 2),
                IncidentsPerMonth = Math.Round(incidentsPerMonth, 2)
            };
        }).ToList();

        var response = new ResponseWrapper<List<DeviceIncidentRateDto>>(200, result);
        return Ok(response);
    }
}
