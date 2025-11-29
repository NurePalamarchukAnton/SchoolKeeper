using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolKeeper.Abstractions.Interfaces.Repository;
using SchoolKeeper.Authorization;
using SchoolKeeper.DTO;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Response;
using SchoolKeeper.Services;

namespace SchoolKeeper.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IncidentController : BaseController<Incident, IncidentDto>
    {
        private readonly IUserService _userService;
        private readonly IDataFilterService _dataFilterService;
        private readonly SchoolKeeperDbContext _context;

        public IncidentController(
            IGenericRepository<Incident> repo,
            IUserService userService,
            IDataFilterService dataFilterService,
            SchoolKeeperDbContext context) 
            : base(repo)
        {
            _userService = userService;
            _dataFilterService = dataFilterService;
            _context = context;
        }

        public override async Task<ActionResult<ResponseWrapper<IEnumerable<IncidentDto>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext);
            if (user == null) throw new UnauthorizedException();

            var query = _context.Incidents
                .Include(i => i.Device)
                .Include(i => i.Reporter)
                .Include(i => i.UserIncidents)
                .AsQueryable();

            // Filter by school (except Admin)
            if (user.Role != UserRole.Admin)
            {
                query = _dataFilterService.FilterBySchool(query, user.SchoolId);
            }

            // Filter by role-specific rules
            query = _dataFilterService.FilterIncidentsByRole(query, user.Role.ToString(), user.Id);

            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 500) pageSize = 50;

            var items = await query
                .OrderByDescending(i => i.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Select(MapToDto).ToList();
            var response = new ResponseWrapper<IEnumerable<IncidentDto>>(200, dtos);
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public override async Task<ActionResult<ResponseWrapper<IncidentDto>>> GetById(int id)
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext);
            if (user == null) throw new UnauthorizedException();

            var incident = await _context.Incidents
                .Include(i => i.Device)
                .Include(i => i.Reporter)
                .Include(i => i.UserIncidents)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (incident == null) throw new NotFoundException();

            // Check access
            // Если у инцидента нет SchoolId, доступ есть только у Admin
            if (user.Role != UserRole.Admin)
            {
                if (!incident.SchoolId.HasValue || incident.SchoolId.Value != user.SchoolId)
                {
                    throw new UnauthorizedException("You don't have access to this incident.");
                }
            }

            // Role-specific access checks
            if (user.Role == UserRole.Teacher)
            {
                if (incident.ReportedBy != user.Id)
                {
                    throw new UnauthorizedException("You can only view your own incidents.");
                }
            }
            else if (user.Role == UserRole.Student)
            {
                // Student can view incidents where they participate via UserIncident
                var isParticipant = await _context.UserIncidents
                    .AnyAsync(ui => ui.IncidentId == id && ui.UserId == user.Id);
                
                if (!isParticipant)
                {
                    throw new UnauthorizedException("You can only view incidents where you participate.");
                }
            }

            var dto = MapToDto(incident);
            
            // Загружаем участников инцидента
            var userIncidents = await _context.UserIncidents
                .Include(ui => ui.User)
                .Where(ui => ui.IncidentId == id)
                .ToListAsync();
            
            dto.Users = userIncidents.Select(ui => new IncidentUserDto
            {
                UserId = ui.UserId,
                FullName = ui.User.FullName,
                Email = ui.User.Email,
                Role = ui.User.Role.ToString()
            }).ToList();
            
            var response = new ResponseWrapper<IncidentDto>(200, dto);
            return Ok(response);
        }

        [HttpPost]
        public override async Task<ActionResult<ResponseWrapper<IncidentDto>>> Create([FromBody] IncidentDto dto)
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext);
            if (user == null) throw new UnauthorizedException();

            // Согласно матрице: только Admin и Security
            if (user.Role != UserRole.Admin && user.Role != UserRole.Security)
            {
                throw new UnauthorizedException("You don't have permission to create incidents.");
            }

            // Set reported by to current user
            dto.ReportedBy = user.Id;

            // Non-admin users can only create incidents for their school
            // Если SchoolId не указан (null), только Admin может создавать такие инциденты
            if (user.Role != UserRole.Admin)
            {
                if (!dto.SchoolId.HasValue || dto.SchoolId.Value != user.SchoolId)
                {
                    throw new BadRequestException("You can only create incidents for your school.");
                }
            }

            return await base.Create(dto);
        }

        [HttpPost("wokwi")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseWrapper<IncidentDto>>> CreateFromWokwi([FromBody] WokwiIncidentDto dto)
        {
            // Эндпоинт для WOKWI IoT устройств без авторизации
            if (dto == null)
            {
                throw new BadRequestException("Request body cannot be null.");
            }

            // Валидация обязательных полей
            if (string.IsNullOrWhiteSpace(dto.DeviceGuid))
            {
                throw new BadRequestException("DeviceGuid is required.");
            }

            if (string.IsNullOrEmpty(dto.IncidentType))
            {
                throw new BadRequestException("IncidentType is required.");
            }

            // Поиск устройства по GUID
            var device = await _context.Devices.FirstOrDefaultAsync(d => d.DeviceGuid == dto.DeviceGuid);

            // Если устройство не найдено - создаем его без SchoolId (админ назначит позже)
            if (device == null)
            {
                // Определяем тип устройства на основе типа инцидента
                DeviceType deviceType = dto.IncidentType switch
                {
                    "MotionSensor" => DeviceType.MotionSensor,
                    "AlarmButton" => DeviceType.AlarmButton,
                    "AccessControl" => DeviceType.AccessControl,
                    _ => DeviceType.MotionSensor // По умолчанию
                };

                // Создаем новое устройство без SchoolId
                device = new Device
                {
                    DeviceName = $"WOKWI Device {dto.DeviceGuid.Substring(0, Math.Min(8, dto.DeviceGuid.Length))}",
                    DeviceType = deviceType,
                    Status = DeviceStatus.Active,
                    Location = "Auto-created from WOKWI",
                    DeviceGuid = dto.DeviceGuid,
                    SchoolId = null // Админ назначит позже
                };

                _context.Devices.Add(device);
                await _context.SaveChangesAsync();
            }

            // Проверяем, что у устройства установлен SchoolId
            // Если SchoolId не установлен, инциденты не создаются - админ должен сначала назначить школу устройству
            if (!device.SchoolId.HasValue)
            {
                throw new BadRequestException("Device does not have a school assigned. Please assign a school to the device before creating incidents.");
            }

            // Определяем SchoolId для инцидента из устройства
            int schoolIdForIncident = device.SchoolId.Value;

            // Находим пользователя для ReportedBy
            // Автоматически находим пользователя Security для школы
            // Если Security нет, используем Admin для этой школы
            var reporter = await _context.Users
                .FirstOrDefaultAsync(u => u.SchoolId == schoolIdForIncident && u.RoleValue == UserRole.Security.ToString())
                ?? await _context.Users
                .FirstOrDefaultAsync(u => u.SchoolId == schoolIdForIncident && u.RoleValue == UserRole.Admin.ToString());

            if (reporter == null)
            {
                throw new NotFoundException($"No Security or Admin user found for school ID {schoolIdForIncident}.");
            }

            // Конвертация строки severity в enum
            IncidentSeverity severity;
            if (!Enum.TryParse<IncidentSeverity>(dto.Severity, ignoreCase: true, out severity))
            {
                throw new BadRequestException($"Invalid severity value: {dto.Severity}. Valid values are: Low, Medium, High, Critical");
            }

            // Конвертация строки status в enum
            IncidentStatus status;
            if (!Enum.TryParse<IncidentStatus>(dto.Status, ignoreCase: true, out status))
            {
                throw new BadRequestException($"Invalid status value: {dto.Status}. Valid values are: Active, Resolved");
            }

            // Установка timestamp, если не указан
            var timestamp = dto.Timestamp ?? DateTime.UtcNow;

            // Создание сущности инцидента
            // SchoolId берется из устройства (уже проверено, что он установлен)
            var incident = new Incident
            {
                DeviceId = device.Id,
                ReportedBy = reporter.Id,
                IncidentType = dto.IncidentType,
                Severity = severity,
                Description = dto.Description,
                Timestamp = timestamp,
                Status = status,
                SchoolId = schoolIdForIncident
            };

            // Сохранение в базу данных
            var created = await Repo.AddAsync(incident);

            // Преобразование в DTO для ответа
            var createdDto = MapToDto(created);
            var response = new ResponseWrapper<IncidentDto>(201, createdDto, "Incident created successfully");
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public override async Task<ActionResult<ResponseWrapper<IncidentDto>>> Update(int id, [FromBody] IncidentDto dto)
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext);
            if (user == null) throw new UnauthorizedException();

            // Проверяем права доступа: Admin, Security или Teacher
            if (user.Role != UserRole.Admin && user.Role != UserRole.Security && user.Role != UserRole.Teacher)
            {
                throw new UnauthorizedException("You don't have permission to update incidents.");
            }

            var incident = await Repo.GetByIdAsync(id);
            if (incident == null) throw new NotFoundException();

            // Check school access
            // Если у инцидента нет SchoolId, доступ есть только у Admin
            if (user.Role != UserRole.Admin)
            {
                if (!incident.SchoolId.HasValue || incident.SchoolId.Value != user.SchoolId)
                {
                    throw new UnauthorizedException("You don't have access to this incident.");
                }
            }

            // Teacher может редактировать инциденты своей школы
            // (не только свои, но и все инциденты школы)

            return await base.Update(id, dto);
        }

        [HttpPost("{id:int}/resolve")]
        public async Task<ActionResult<ResponseWrapper<IncidentDto>>> Resolve(int id)
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext);
            if (user == null) throw new UnauthorizedException();

            // Согласно матрице: только Admin и Teacher
            if (user.Role != UserRole.Admin && user.Role != UserRole.Teacher)
            {
                throw new UnauthorizedException("You don't have permission to resolve incidents.");
            }

            var incident = await _context.Incidents.FirstOrDefaultAsync(i => i.Id == id);
            if (incident == null) throw new NotFoundException();

            // Check school access
            // Если у инцидента нет SchoolId, доступ есть только у Admin
            if (user.Role != UserRole.Admin)
            {
                if (!incident.SchoolId.HasValue || incident.SchoolId.Value != user.SchoolId)
                {
                    throw new UnauthorizedException("You don't have access to this incident.");
                }
            }

            // Teachers can only resolve their own incidents
            if (user.Role == UserRole.Teacher && incident.ReportedBy != user.Id)
            {
                throw new UnauthorizedException("You can only resolve your own incidents.");
            }

            incident.Status = IncidentStatus.Resolved;
            await _context.SaveChangesAsync();

            var dto = MapToDto(incident);
            var response = new ResponseWrapper<IncidentDto>(200, dto, "Incident resolved successfully");
            return Ok(response);
        }

        // ======== Добавление пользователя к инциденту ========
        [HttpPost("{id:int}/add-user")]
        [Authorize]
        public async Task<ActionResult<ResponseWrapper<object>>> AddUserToIncident(int id, [FromBody] AddUserToIncidentDto dto)
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext);
            if (user == null) throw new UnauthorizedException();

            // Проверяем права доступа: Admin, Security или Teacher
            if (user.Role != UserRole.Admin && user.Role != UserRole.Security && user.Role != UserRole.Teacher)
            {
                throw new UnauthorizedException("You don't have permission to add users to incidents.");
            }

            var incident = await _context.Incidents
                .Include(i => i.UserIncidents)
                .FirstOrDefaultAsync(i => i.Id == id);
            
            if (incident == null) throw new NotFoundException("Incident not found");

            // Check school access
            if (user.Role != UserRole.Admin)
            {
                if (!incident.SchoolId.HasValue || incident.SchoolId.Value != user.SchoolId)
                {
                    throw new UnauthorizedException("You don't have access to this incident.");
                }
            }

            // Проверяем, что пользователь существует и принадлежит той же школе
            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
            if (targetUser == null) throw new NotFoundException("User not found");

            if (user.Role != UserRole.Admin && targetUser.SchoolId != user.SchoolId)
            {
                throw new UnauthorizedException("You can only add users from your school to incidents.");
            }

            // Проверяем, не добавлен ли уже пользователь к инциденту
            var existing = await _context.UserIncidents
                .FirstOrDefaultAsync(ui => ui.IncidentId == id && ui.UserId == dto.UserId);

            if (existing != null)
            {
                throw new BadRequestException("User is already added to this incident.");
            }

            // Добавляем пользователя к инциденту
            var userIncident = new UserIncident
            {
                UserId = dto.UserId,
                IncidentId = id
            };

            _context.UserIncidents.Add(userIncident);
            await _context.SaveChangesAsync();

            var response = new ResponseWrapper<object>(200, null, "User added to incident successfully");
            return Ok(response);
        }

        // ======== Удаление пользователя из инцидента ========
        [HttpDelete("{id:int}/remove-user/{userId:int}")]
        [Authorize]
        public async Task<ActionResult<ResponseWrapper<object>>> RemoveUserFromIncident(int id, int userId)
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext);
            if (user == null) throw new UnauthorizedException();

            // Проверяем права доступа: Admin, Security или Teacher
            if (user.Role != UserRole.Admin && user.Role != UserRole.Security && user.Role != UserRole.Teacher)
            {
                throw new UnauthorizedException("You don't have permission to remove users from incidents.");
            }

            var incident = await _context.Incidents.FirstOrDefaultAsync(i => i.Id == id);
            if (incident == null) throw new NotFoundException("Incident not found");

            // Check school access
            if (user.Role != UserRole.Admin)
            {
                if (!incident.SchoolId.HasValue || incident.SchoolId.Value != user.SchoolId)
                {
                    throw new UnauthorizedException("You don't have access to this incident.");
                }
            }

            var userIncident = await _context.UserIncidents
                .FirstOrDefaultAsync(ui => ui.IncidentId == id && ui.UserId == userId);

            if (userIncident == null)
            {
                throw new NotFoundException("User is not associated with this incident.");
            }

            _context.UserIncidents.Remove(userIncident);
            await _context.SaveChangesAsync();

            var response = new ResponseWrapper<object>(200, null, "User removed from incident successfully");
            return Ok(response);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public override async Task<ActionResult<ResponseWrapper<object>>> Delete(int id)
        {
            return await base.Delete(id);
        }
    }
}
