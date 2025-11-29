using Microsoft.EntityFrameworkCore;
using SchoolKeeper.Models;
using SchoolKeeper.Models.Enums;

namespace SchoolKeeper.Services;

public class DbSeeder
{
    private readonly SchoolKeeperDbContext _context;

    public DbSeeder(SchoolKeeperDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Проверяем, есть ли уже данные
        if (await _context.Schools.AnyAsync())
        {
            return; // База уже заполнена
        }

        // 1. Создаем школы
        var schools = new List<School>
        {
            new School
            {
                Name = "Школа №1 ім. Тараса Шевченка",
                Address = "вул. Хрещатик, 1, Київ",
                Region = "Київська область",
                ContactNumber = "+380441234567"
            },
            new School
            {
                Name = "Гімназія №2",
                Address = "вул. Саксаганського, 15, Львів",
                Region = "Львівська область",
                ContactNumber = "+380321234567"
            },
            new School
            {
                Name = "Ліцей №3",
                Address = "просп. Перемоги, 25, Одеса",
                Region = "Одеська область",
                ContactNumber = "+380481234567"
            }
        };

        await _context.Schools.AddRangeAsync(schools);
        await _context.SaveChangesAsync();

        // 2. Создаем пользователей для каждой школы
        var users = new List<User>();
        var defaultPassword = BCrypt.Net.BCrypt.HashPassword("123456"); // Пароль по умолчанию

        foreach (var school in schools)
        {
            // Администратор
            users.Add(new User
            {
                FullName = $"Адміністратор {school.Name}",
                Email = $"admin{school.Id}@school.edu.ua",
                PasswordHash = defaultPassword,
                Role = UserRole.Admin,
                PhoneNumber = $"+380{50 + school.Id}1234567",
                SchoolId = school.Id
            });

            // Охранники
            for (int i = 1; i <= 2; i++)
            {
                users.Add(new User
                {
                    FullName = $"Охоронець {i} - {school.Name}",
                    Email = $"security{school.Id}_{i}@school.edu.ua",
                    PasswordHash = defaultPassword,
                    Role = UserRole.Security,
                    PhoneNumber = $"+380{60 + school.Id + i}1234567",
                    SchoolId = school.Id
                });
            }

            // Учителя
            var teacherNames = new[]
            {
                "Іван Петренко", "Марія Коваленко", "Олександр Бондаренко",
                "Наталія Шевченко", "Василь Мельник", "Оксана Ткаченко"
            };

            for (int i = 0; i < teacherNames.Length; i++)
            {
                users.Add(new User
                {
                    FullName = $"{teacherNames[i]} - {school.Name}",
                    Email = $"teacher{school.Id}_{i + 1}@school.edu.ua",
                    PasswordHash = defaultPassword,
                    Role = UserRole.Teacher,
                    PhoneNumber = $"+380{70 + school.Id + i}1234567",
                    SchoolId = school.Id
                });
            }

            // Родители
            var parentNames = new[]
            {
                "Олена Іваненко", "Петро Сидоренко", "Тетяна Гриценко",
                "Михайло Лисенко", "Юлія Романенко"
            };

            for (int i = 0; i < parentNames.Length; i++)
            {
                users.Add(new User
                {
                    FullName = $"{parentNames[i]} - {school.Name}",
                    Email = $"parent{school.Id}_{i + 1}@school.edu.ua",
                    PasswordHash = defaultPassword,
                    Role = UserRole.Parent,
                    PhoneNumber = $"+380{80 + school.Id + i}1234567",
                    SchoolId = school.Id
                });
            }

            // Студенты
            var studentNames = new[]
            {
                "Андрій Іваненко", "Софія Сидоренко", "Дмитро Гриценко",
                "Анна Лисенко", "Максим Романенко", "Вікторія Петренко",
                "Олексій Коваленко", "Катерина Бондаренко", "Ігор Шевченко",
                "Марія Мельник"
            };

            for (int i = 0; i < studentNames.Length; i++)
            {
                users.Add(new User
                {
                    FullName = $"{studentNames[i]} - {school.Name}",
                    Email = $"student{school.Id}_{i + 1}@school.edu.ua",
                    PasswordHash = defaultPassword,
                    Role = UserRole.Student,
                    PhoneNumber = $"+380{90 + school.Id + i}1234567",
                    SchoolId = school.Id
                });
            }
        }

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // 3. Создаем устройства для каждой школы
        var devices = new List<Device>();
        var deviceTypes = Enum.GetValues<DeviceType>();
        var deviceStatuses = Enum.GetValues<DeviceStatus>();
        var locations = new[] { "Головний вхід", "Шкільний двір", "Спортивний зал", "Столова", "Бібліотека", "Коридор 1-й поверх", "Коридор 2-й поверх" };

        foreach (var school in schools)
        {
            var random = new Random(school.Id);
            for (int i = 0; i < 10; i++)
            {
                devices.Add(new Device
                {
                    DeviceName = $"Пристрій {i + 1} - {school.Name}",
                    DeviceType = deviceTypes[random.Next(deviceTypes.Length)],
                    Status = deviceStatuses[random.Next(deviceStatuses.Length)],
                    Location = locations[random.Next(locations.Length)],
                    SchoolId = school.Id
                });
            }
        }

        await _context.Devices.AddRangeAsync(devices);
        await _context.SaveChangesAsync();

        // 4. Создаем связи Parent-Student
        var parentStudents = new List<ParentStudent>();
        foreach (var school in schools)
        {
            var schoolParents = users.Where(u => u.SchoolId == school.Id && u.Role == UserRole.Parent).ToList();
            var schoolStudents = users.Where(u => u.SchoolId == school.Id && u.Role == UserRole.Student).ToList();

            // Каждому родителю назначаем 1-2 студентов
            for (int i = 0; i < schoolParents.Count && i * 2 < schoolStudents.Count; i++)
            {
                var parent = schoolParents[i];
                // Первый студент
                parentStudents.Add(new ParentStudent
                {
                    ParentId = parent.Id,
                    StudentId = schoolStudents[i * 2].Id
                });

                // Второй студент (если есть)
                if (i * 2 + 1 < schoolStudents.Count)
                {
                    parentStudents.Add(new ParentStudent
                    {
                        ParentId = parent.Id,
                        StudentId = schoolStudents[i * 2 + 1].Id
                    });
                }
            }
        }

        await _context.ParentStudents.AddRangeAsync(parentStudents);
        await _context.SaveChangesAsync();

        // 5. Создаем связи Student-Teacher
        var studentTeachers = new List<StudentTeacher>();
        foreach (var school in schools)
        {
            var schoolTeachers = users.Where(u => u.SchoolId == school.Id && u.Role == UserRole.Teacher).ToList();
            var schoolStudents = users.Where(u => u.SchoolId == school.Id && u.Role == UserRole.Student).ToList();

            // Каждому студенту назначаем 2-3 учителей
            var random = new Random(school.Id);
            foreach (var student in schoolStudents)
            {
                var teacherCount = random.Next(2, 4);
                var selectedTeachers = schoolTeachers.OrderBy(x => random.Next()).Take(teacherCount).ToList();

                foreach (var teacher in selectedTeachers)
                {
                    studentTeachers.Add(new StudentTeacher
                    {
                        StudentId = student.Id,
                        TeacherId = teacher.Id
                    });
                }
            }
        }

        await _context.StudentTeachers.AddRangeAsync(studentTeachers);
        await _context.SaveChangesAsync();

        // 6. Создаем инциденты
        var incidents = new List<Incident>();
        var incidentTypes = new[] { "Порушення дисципліни", "Технічна несправність", "Підозріла активність", "Аварія", "Пожежа" };
        var severities = Enum.GetValues<IncidentSeverity>();
        var statuses = Enum.GetValues<IncidentStatus>();

        foreach (var school in schools)
        {
            var schoolSecurity = users.Where(u => u.SchoolId == school.Id && u.Role == UserRole.Security).ToList();
            var schoolDevices = devices.Where(d => d.SchoolId == school.Id).ToList();
            var random = new Random(school.Id + 100);

            for (int i = 0; i < 15; i++)
            {
                var reportedBy = schoolSecurity[random.Next(schoolSecurity.Count)];
                var device = schoolDevices[random.Next(schoolDevices.Count)];
                var timestamp = DateTime.UtcNow.AddDays(-random.Next(30)).AddHours(-random.Next(24));

                incidents.Add(new Incident
                {
                    DeviceId = device.Id,
                    ReportedBy = reportedBy.Id,
                    IncidentType = incidentTypes[random.Next(incidentTypes.Length)],
                    Severity = severities[random.Next(severities.Length)],
                    Description = $"Опис інциденту #{i + 1} в {school.Name}. Детальна інформація про подію.",
                    Timestamp = timestamp,
                    Status = statuses[random.Next(statuses.Length)],
                    SchoolId = school.Id
                });
            }
        }

        await _context.Incidents.AddRangeAsync(incidents);
        await _context.SaveChangesAsync();

        // 7. Создаем связи UserIncident (студенты участвуют в инцидентах)
        var userIncidents = new List<UserIncident>();
        foreach (var school in schools)
        {
            var schoolStudents = users.Where(u => u.SchoolId == school.Id && u.Role == UserRole.Student).ToList();
            var schoolIncidents = incidents.Where(i => i.SchoolId == school.Id).ToList();
            var random = new Random(school.Id + 200);

            // Каждому инциденту назначаем 1-3 студентов
            foreach (var incident in schoolIncidents)
            {
                var studentCount = random.Next(1, 4);
                var selectedStudents = schoolStudents.OrderBy(x => random.Next()).Take(studentCount).ToList();

                foreach (var student in selectedStudents)
                {
                    userIncidents.Add(new UserIncident
                    {
                        UserId = student.Id,
                        IncidentId = incident.Id
                    });
                }
            }
        }

        await _context.UserIncidents.AddRangeAsync(userIncidents);
        await _context.SaveChangesAsync();

        // 8. Создаем отчеты
        var reports = new List<Rept>();
        foreach (var school in schools)
        {
            var schoolAdmin = users.FirstOrDefault(u => u.SchoolId == school.Id && u.Role == UserRole.Admin);
            if (schoolAdmin == null) continue;

            var random = new Random(school.Id + 300);
            for (int i = 0; i < 5; i++)
            {
                var periodStart = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-i - 1));
                var periodEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-i));

                reports.Add(new Rept
                {
                    SchoolId = school.Id,
                    GeneratedBy = schoolAdmin.Id,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    Summary = $"Звіт за період {periodStart:dd.MM.yyyy} - {periodEnd:dd.MM.yyyy} для {school.Name}",
                    GeneratedOn = DateTime.UtcNow.AddDays(-i * 7)
                });
            }
        }

        await _context.Reports.AddRangeAsync(reports);
        await _context.SaveChangesAsync();

        // 9. Создаем связи ReptIncident (инциденты в отчетах)
        var reptIncidents = new List<ReptIncident>();
        foreach (var report in reports)
        {
            var schoolIncidents = incidents.Where(i => i.SchoolId == report.SchoolId).ToList();
            var random = new Random(report.Id);

            // В каждый отчет добавляем 3-8 инцидентов
            var incidentCount = random.Next(3, 9);
            var selectedIncidents = schoolIncidents.OrderBy(x => random.Next()).Take(incidentCount).ToList();

            foreach (var incident in selectedIncidents)
            {
                reptIncidents.Add(new ReptIncident
                {
                    ReptId = report.Id,
                    IncidentId = incident.Id
                });
            }
        }

        await _context.ReptIncidents.AddRangeAsync(reptIncidents);
        await _context.SaveChangesAsync();
    }
}

