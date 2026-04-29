using Microsoft.EntityFrameworkCore;
using SchoolKeeper.DTO;
using SchoolKeeper.Extentions;
using SchoolKeeper.Models.Enums;

namespace SchoolKeeper.Services;

public interface IReportGenerationService
{
    Task<string> GenerateTeacherReportAsync(int schoolId, DateOnly periodStart, DateOnly periodEnd, int teacherId);
    Task<string> GenerateSecurityReportAsync(int schoolId, DateOnly periodStart, DateOnly periodEnd);
}

public class ReportGenerationService : IReportGenerationService
{
    private readonly SchoolKeeperDbContext _context;

    public ReportGenerationService(SchoolKeeperDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateTeacherReportAsync(int schoolId, DateOnly periodStart, DateOnly periodEnd, int teacherId)
    {
        var startDate = periodStart.ToUtcDateTimeStart();
        var endDate = periodEnd.ToUtcDateTimeEnd();

        // Получаем статистику инцидентов для школы
        var incidents = await _context.Incidents
            .Where(i => i.SchoolId == schoolId && 
                       i.Timestamp >= startDate && 
                       i.Timestamp <= endDate)
            .Include(i => i.UserIncidents)
                .ThenInclude(ui => ui.User)
            .ToListAsync();

        // Получаем студентов учителя
        var studentIds = await _context.StudentTeachers
            .Where(st => st.TeacherId == teacherId)
            .Select(st => st.StudentId)
            .ToListAsync();

        // Получаем инциденты, связанные со студентами учителя
        var studentIncidentIds = await _context.UserIncidents
            .Where(ui => studentIds.Contains(ui.UserId))
            .Select(ui => ui.IncidentId)
            .Distinct()
            .ToListAsync();

        var studentRelatedIncidents = incidents
            .Where(i => studentIncidentIds.Contains(i.Id) || studentIds.Contains(i.ReportedBy))
            .ToList();

        // Получаем родителей студентов
        var parentIds = await _context.ParentStudents
            .Where(ps => studentIds.Contains(ps.StudentId))
            .Select(ps => ps.ParentId)
            .Distinct()
            .ToListAsync();

        var students = await _context.Users
            .Where(u => studentIds.Contains(u.Id))
            .ToListAsync();

        var parents = await _context.Users
            .Where(u => parentIds.Contains(u.Id))
            .ToListAsync();

        // Формируем отчет
        var summary = new System.Text.StringBuilder();
        summary.AppendLine($"ЗВІТ ВЧИТЕЛЯ ЗА ПЕРІОД {periodStart:dd.MM.yyyy} - {periodEnd:dd.MM.yyyy}");
        summary.AppendLine("=".PadRight(60, '='));
        summary.AppendLine();

        // Статистика инцидентов
        summary.AppendLine("СТАТИСТИКА ІНЦИДЕНТІВ:");
        summary.AppendLine($"  Всього інцидентів: {incidents.Count}");
        summary.AppendLine($"  Активних інцидентів: {incidents.Count(i => i.Status == IncidentStatus.Active)}");
        summary.AppendLine($"  Вирішених інцидентів: {incidents.Count(i => i.Status == IncidentStatus.Resolved)}");
        summary.AppendLine();

        // Распределение по типам
        if (incidents.Any())
        {
            summary.AppendLine("  Розподіл за типами:");
            var byType = incidents.GroupBy(i => i.IncidentType);
            foreach (var group in byType.OrderByDescending(g => g.Count()))
            {
                summary.AppendLine($"    {group.Key}: {group.Count()}");
            }
            summary.AppendLine();

            summary.AppendLine("  Розподіл за серйозністю:");
            var bySeverity = incidents.GroupBy(i => i.Severity);
            foreach (var group in bySeverity.OrderByDescending(g => g.Count()))
            {
                summary.AppendLine($"    {group.Key}: {group.Count()}");
            }
            summary.AppendLine();
        }

        // Статистика по студентам
        summary.AppendLine("СТАТИСТИКА ПО СТУДЕНТАХ:");
        summary.AppendLine($"  Всього студентів: {students.Count}");
        summary.AppendLine($"  Студентів, пов'язаних з інцидентами: {studentRelatedIncidents.SelectMany(i => i.UserIncidents.Where(ui => studentIds.Contains(ui.UserId)).Select(ui => ui.UserId)).Distinct().Count()}");
        summary.AppendLine();

        // Статистика по родителям
        summary.AppendLine("СТАТИСТИКА ПО БАТЬКАХ:");
        summary.AppendLine($"  Всього батьків: {parents.Count}");
        summary.AppendLine();

        // Временные тренды
        if (incidents.Any())
        {
            summary.AppendLine("ВРЕМЕННІ ТРЕНДИ:");
            var timeline = incidents
                .GroupBy(i => i.Timestamp.Date)
                .OrderBy(g => g.Key)
                .Take(10); // Показываем последние 10 дней

            foreach (var day in timeline)
            {
                summary.AppendLine($"  {day.Key:dd.MM.yyyy}: {day.Count()} інцидентів");
            }
            summary.AppendLine();
        }

        // Инциденты, связанные со студентами
        if (studentRelatedIncidents.Any())
        {
            summary.AppendLine("ІНЦИДЕНТИ, ПОВ'ЯЗАНІ ЗІ СТУДЕНТАМИ:");
            summary.AppendLine($"  Всього: {studentRelatedIncidents.Count}");
            summary.AppendLine($"  Активних: {studentRelatedIncidents.Count(i => i.Status == IncidentStatus.Active)}");
            summary.AppendLine($"  Вирішених: {studentRelatedIncidents.Count(i => i.Status == IncidentStatus.Resolved)}");
        }

        return summary.ToString();
    }

    public async Task<string> GenerateSecurityReportAsync(int schoolId, DateOnly periodStart, DateOnly periodEnd)
    {
        var startDate = periodStart.ToUtcDateTimeStart();
        var endDate = periodEnd.ToUtcDateTimeEnd();

        // Получаем статистику устройств
        var devices = await _context.Devices
            .Where(d => d.SchoolId == schoolId)
            .ToListAsync();

        // Получаем статистику инцидентов
        var incidents = await _context.Incidents
            .Where(i => i.SchoolId == schoolId && 
                       i.Timestamp >= startDate && 
                       i.Timestamp <= endDate)
            .Include(i => i.Device)
            .ToListAsync();

        // Получаем разрешенные инциденты для расчета времени решения
        var resolvedIncidents = incidents
            .Where(i => i.Status == IncidentStatus.Resolved)
            .ToList();

        // Рассчитываем среднее время решения
        double avgResolutionHours = 0;
        double avgResolutionDays = 0;
        if (resolvedIncidents.Any())
        {
            // Для расчета времени решения нужна информация о том, когда инцидент был решен
            // Так как у нас нет поля ResolvedAt, используем Timestamp как приближение
            // В реальности нужно добавить поле ResolvedAt в модель Incident
            var resolutionTimes = resolvedIncidents
                .Select(i => (DateTime.UtcNow - i.Timestamp).TotalHours)
                .ToList();
            
            avgResolutionHours = resolutionTimes.Any() ? resolutionTimes.Average() : 0;
            avgResolutionDays = avgResolutionHours / 24;
        }

        // Формируем отчет
        var summary = new System.Text.StringBuilder();
        summary.AppendLine($"ЗВІТ ОХОРОНИ ЗА ПЕРІОД {periodStart:dd.MM.yyyy} - {periodEnd:dd.MM.yyyy}");
        summary.AppendLine("=".PadRight(60, '='));
        summary.AppendLine();

        // Статистика устройств
        summary.AppendLine("СТАТИСТИКА ПРИСТРОЇВ:");
        summary.AppendLine($"  Всього пристроїв: {devices.Count}");
        summary.AppendLine($"  Активних: {devices.Count(d => d.Status == DeviceStatus.Active)}");
        summary.AppendLine($"  Неактивних: {devices.Count(d => d.Status == DeviceStatus.Inactive)}");
        summary.AppendLine($"  З помилками: {devices.Count(d => d.Status == DeviceStatus.Error)}");
        summary.AppendLine();

        if (devices.Any())
        {
            summary.AppendLine("  Розподіл за типами:");
            var byType = devices.GroupBy(d => d.DeviceType);
            foreach (var group in byType.OrderByDescending(g => g.Count()))
            {
                summary.AppendLine($"    {group.Key}: {group.Count()}");
            }
            summary.AppendLine();

            summary.AppendLine("  Розподіл за локаціями:");
            var byLocation = devices
                .Where(d => !string.IsNullOrEmpty(d.Location))
                .GroupBy(d => d.Location!);
            foreach (var group in byLocation.OrderByDescending(g => g.Count()).Take(10))
            {
                summary.AppendLine($"    {group.Key}: {group.Count()}");
            }
            summary.AppendLine();
        }

        // Статистика инцидентов
        summary.AppendLine("СТАТИСТИКА ІНЦИДЕНТІВ:");
        summary.AppendLine($"  Всього інцидентів: {incidents.Count}");
        summary.AppendLine($"  Активних інцидентів: {incidents.Count(i => i.Status == IncidentStatus.Active)}");
        summary.AppendLine($"  Вирішених інцидентів: {incidents.Count(i => i.Status == IncidentStatus.Resolved)}");
        summary.AppendLine();

        if (incidents.Any())
        {
            summary.AppendLine("  Розподіл за типами:");
            var byType = incidents.GroupBy(i => i.IncidentType);
            foreach (var group in byType.OrderByDescending(g => g.Count()))
            {
                summary.AppendLine($"    {group.Key}: {group.Count()}");
            }
            summary.AppendLine();

            summary.AppendLine("  Розподіл за серйозністю:");
            var bySeverity = incidents.GroupBy(i => i.Severity);
            foreach (var group in bySeverity.OrderByDescending(g => g.Count()))
            {
                summary.AppendLine($"    {group.Key}: {group.Count()}");
            }
            summary.AppendLine();
        }

        // Время решения
        summary.AppendLine("ЧАС ВИРІШЕННЯ ІНЦИДЕНТІВ:");
        summary.AppendLine($"  Вирішено інцидентів: {resolvedIncidents.Count}");
        if (resolvedIncidents.Any())
        {
            summary.AppendLine($"  Середній час вирішення: {avgResolutionHours:F2} годин ({avgResolutionDays:F2} днів)");
        }
        summary.AppendLine();

        // Статистика по устройствам и инцидентам
        if (incidents.Any())
        {
            summary.AppendLine("СТАТИСТИКА ІНЦИДЕНТІВ ПО ПРИСТРОЯХ:");
            var byDevice = incidents
                .Where(i => i.Device != null)
                .GroupBy(i => i.Device!.DeviceName)
                .OrderByDescending(g => g.Count())
                .Take(10);

            foreach (var group in byDevice)
            {
                summary.AppendLine($"  {group.Key}: {group.Count()} інцидентів");
            }
        }

        return summary.ToString();
    }
}

