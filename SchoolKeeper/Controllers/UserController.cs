using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
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
    public class UserController : BaseController<User, UserDto>
    {
        private readonly IUserService _userService;
        private readonly IDataFilterService _dataFilterService;
        private readonly SchoolKeeperDbContext _context;

        public UserController(
            IGenericRepository<User> repo,
            IUserService userService,
            IDataFilterService dataFilterService,
            SchoolKeeperDbContext context) 
            : base(repo)
        {
            _userService = userService;
            _dataFilterService = dataFilterService;
            _context = context;
        }

        public override async Task<ActionResult<ResponseWrapper<IEnumerable<UserDto>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext);
            if (user == null) throw new UnauthorizedException();

            var query = _context.Users
                .Include(u => u.School)
                .AsQueryable();

            // Filter by school (except Admin)
            if (user.Role != UserRole.Admin)
            {
                query = _dataFilterService.FilterBySchool(query, user.SchoolId);
            }

            // Filter by role-specific rules
            query = _dataFilterService.FilterUsersByRole(query, user.Role.ToString(), user.Id);

            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 500) pageSize = 50;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Select(MapToDto).ToList();
            var response = new ResponseWrapper<IEnumerable<UserDto>>(200, dtos);
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public override async Task<ActionResult<ResponseWrapper<UserDto>>> GetById(int id)
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext);
            if (user == null) throw new UnauthorizedException();

            var targetUser = await _context.Users
                .Include(u => u.School)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (targetUser == null) throw new NotFoundException();

            // Согласно матрице: Admin, Security, Teacher
            if (user.Role != UserRole.Admin && user.Role != UserRole.Security && user.Role != UserRole.Teacher)
            {
                // Others can only see themselves
                if (targetUser.Id != user.Id)
                {
                    throw new UnauthorizedException("You can only view your own profile.");
                }
            }
            else if (user.Role == UserRole.Security || user.Role == UserRole.Teacher)
            {
                // Security and Teacher can only see users from their school
                if (targetUser.SchoolId != user.SchoolId)
                {
                    throw new UnauthorizedException("You don't have access to this user.");
                }
            }

            var dto = MapToDto(targetUser);
            var response = new ResponseWrapper<UserDto>(200, dto);
            return Ok(response);
        }

        // Переопределяем базовый метод Create, скрывая его от Swagger
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public override Task<ActionResult<ResponseWrapper<UserDto>>> Create([FromBody] UserDto dto)
        {
            // Базовый метод скрыт, используется CreateUser ниже
            return Task.FromResult<ActionResult<ResponseWrapper<UserDto>>>(BadRequest(new ResponseWrapper<UserDto>(400, null, "Use CreateUser endpoint")));
        }

        [HttpPost]
        [ActionName("Create")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<ResponseWrapper<UserDto>>> CreateUser([FromBody] UserCreateDto dto)
        {
            if (dto == null) throw new BadRequestException("DTO cannot be null");
            
            // Проверяем, что email уникален
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new BadRequestException("User with this email already exists.");

            // Хешируем пароль
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = hashedPassword,
                Role = dto.Role,
                PhoneNumber = dto.PhoneNumber,
                SchoolId = dto.SchoolId
            };

            await OnBeforeCreate(user);
            var created = await Repo.AddAsync(user);
            await OnAfterCreate(created);

            var createdDto = MapToDto(created);
            var response = new ResponseWrapper<UserDto>(201, createdDto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
        }

        // Переопределяем базовый метод Update, скрывая его от Swagger
        [HttpPut("{id:int}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public override Task<ActionResult<ResponseWrapper<UserDto>>> Update(int id, [FromBody] UserDto dto)
        {
            // Базовый метод скрыт, используется UpdateUser ниже
            return Task.FromResult<ActionResult<ResponseWrapper<UserDto>>>(BadRequest(new ResponseWrapper<UserDto>(400, null, "Use UpdateUser endpoint")));
        }

        [HttpPut("{id:int}")]
        [ActionName("Update")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<ResponseWrapper<UserDto>>> UpdateUser(int id, [FromBody] UserUpdateDto dto)
        {
            if (dto == null) throw new BadRequestException("DTO cannot be null");

            var existing = await Repo.GetByIdAsync(id);
            if (existing == null) throw new NotFoundException();

            // Проверяем уникальность email, если он изменяется
            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != existing.Email)
            {
                if (await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id))
                    throw new BadRequestException("User with this email already exists.");
            }

            // Обновляем поля
            if (!string.IsNullOrEmpty(dto.FullName))
                existing.FullName = dto.FullName;
            
            if (!string.IsNullOrEmpty(dto.Email))
                existing.Email = dto.Email;
            
            if (dto.Role.HasValue)
                existing.Role = dto.Role.Value;
            
            if (dto.PhoneNumber != null)
                existing.PhoneNumber = dto.PhoneNumber;
            
            if (dto.SchoolId.HasValue)
                existing.SchoolId = dto.SchoolId.Value;

            // Обновляем пароль только если он указан
            if (!string.IsNullOrEmpty(dto.Password))
            {
                existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await OnBeforeUpdate(existing, existing);
            var updated = await Repo.UpdateAsync(existing);
            await OnAfterUpdate(updated);

            var updatedDto = MapToDto(updated);
            var response = new ResponseWrapper<UserDto>(200, updatedDto);
            return Ok(response);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public override async Task<ActionResult<ResponseWrapper<object>>> Delete(int id)
        {
            return await base.Delete(id);
        }

        // ======== Получить учителей студента ========
        [HttpGet("MyTeachers")]
        public async Task<ActionResult<ResponseWrapper<IEnumerable<UserDto>>>> GetMyTeachers()
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext);
            if (user == null) throw new UnauthorizedException();

            // Только студенты могут получить своих учителей
            if (user.Role != UserRole.Student)
            {
                throw new UnauthorizedException("Only students can access their teachers.");
            }

            var teachersQuery = _dataFilterService.GetStudentTeachers(user.Id);
            var teachers = await teachersQuery
                .Include(u => u.School)
                .ToListAsync();

            var dtos = teachers.Select(MapToDto).ToList();
            var response = new ResponseWrapper<IEnumerable<UserDto>>(200, dtos);
            return Ok(response);
        }
    }
}
