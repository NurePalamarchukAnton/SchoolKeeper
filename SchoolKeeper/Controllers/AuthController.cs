using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SchoolKeeper.DTO;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Response;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SchoolKeeper.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public partial class AuthController : ControllerBase
    {
        private readonly SchoolKeeperDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(SchoolKeeperDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ======== Регистрация ========
        [HttpPost("register")]
        public async Task<ActionResult<ResponseWrapper<object>>> Register([FromBody] RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new BadRequestException("User with this email already exists.");

            // Хэшируем пароль (упрощённо — для продакшена используй IPasswordHasher)
            var hashed = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = hashed,
                Role = dto.Role,
                SchoolId = dto.SchoolId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = new ResponseWrapper<object>(200, null, "User registered successfully");
            return Ok(response);
        }

        // ======== Логин ========
        [HttpPost("login")]
        public async Task<ActionResult<ResponseWrapper<DTO.AuthResponse>>> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                throw new UnauthorizedException("Invalid email or password.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedException("Invalid email or password.");

            var token = GenerateJwtToken(user);

            var authResponse = new DTO.AuthResponse
            {
                Token = token,
                Email = user.Email,
                Role = user.Role.ToString(),
                UserId = user.Id
            };

            var response = new ResponseWrapper<AuthResponse>(200, authResponse);
            return Ok(response);
        }

        // ======== Impersonation (вход от имени другого пользователя) ========
        [HttpPost("impersonate")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<ActionResult<ResponseWrapper<DTO.AuthResponse>>> Impersonate([FromBody] ImpersonateDto dto)
        {
            // Получаем текущего пользователя (должен быть Admin)
            var adminIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminIdClaim) || !int.TryParse(adminIdClaim, out var adminId))
                throw new UnauthorizedException("Admin authentication required");

            var admin = await _context.Users.FindAsync(adminId);
            if (admin == null || admin.Role != UserRole.Admin)
                throw new UnauthorizedException("Only admins can impersonate users");

            // Получаем пользователя, от имени которого хотим войти
            var targetUser = await _context.Users.FindAsync(dto.UserId);
            if (targetUser == null)
                throw new NotFoundException("User not found");

            // Генерируем токен для целевого пользователя
            var token = GenerateJwtToken(targetUser);

            var authResponse = new DTO.AuthResponse
            {
                Token = token,
                Email = targetUser.Email,
                Role = targetUser.Role.ToString(),
                UserId = targetUser.Id,
                OriginalAdminId = adminId.ToString() // Сохраняем ID админа для выхода из режима
            };

            var response = new ResponseWrapper<AuthResponse>(200, authResponse);
            return Ok(response);
        }

        // ======== Выход из режима impersonation ========
        [HttpPost("stop-impersonation")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<ActionResult<ResponseWrapper<DTO.AuthResponse>>> StopImpersonation([FromBody] StopImpersonationDto? dto = null)
        {
            int adminId;
            
            // Пытаемся получить ID админа из тела запроса (из localStorage)
            if (dto != null && !string.IsNullOrEmpty(dto.OriginalAdminId) && int.TryParse(dto.OriginalAdminId, out adminId))
            {
                // Используем ID из запроса
            }
            // Если нет в запросе, пытаемся получить из cookie
            else if (Request.Cookies.ContainsKey("originalAdminId") && 
                     int.TryParse(Request.Cookies["originalAdminId"], out adminId))
            {
                // Используем ID из cookie
            }
            // Если нет ни там, ни там, пытаемся найти админа по текущему пользователю
            // (если текущий пользователь не админ, значит мы в режиме impersonation)
            else
            {
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out var currentUserId))
                {
                    throw new BadRequestException("No impersonation session found");
                }

                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                {
                    throw new BadRequestException("Current user not found");
                }

                // Если текущий пользователь - админ, значит мы уже не в режиме impersonation
                if (currentUser.Role == UserRole.Admin)
                {
                    throw new BadRequestException("Not in impersonation mode");
                }

                // Ищем админа, который мог создать эту сессию
                // В реальности нужно хранить связь, но для простоты ищем любого админа
                // Лучше всего - сохранять originalAdminId в cookie при входе в режим impersonation
                var foundAdmin = await _context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Admin);
                if (foundAdmin == null)
                {
                    throw new NotFoundException("No admin user found");
                }
                adminId = foundAdmin.Id;
            }

            // Получаем оригинального админа
            var admin = await _context.Users.FindAsync(adminId);
            if (admin == null || admin.Role != UserRole.Admin)
            {
                throw new UnauthorizedException("Original admin not found or invalid");
            }

            // Генерируем новый токен для админа
            var token = GenerateJwtToken(admin);

            var authResponse = new DTO.AuthResponse
            {
                Token = token,
                Email = admin.Email,
                Role = admin.Role.ToString(),
                UserId = admin.Id
                // Не передаем OriginalAdminId, так как выходим из режима impersonation
            };

            var response = new ResponseWrapper<AuthResponse>(200, authResponse, "Returned to admin account");
            return Ok(response);
        }

        // ======== Генерация JWT токена ========
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
