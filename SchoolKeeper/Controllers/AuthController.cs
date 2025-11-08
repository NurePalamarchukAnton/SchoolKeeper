using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SchoolKeeper.Models.Enums;
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
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("User with this email already exists.");

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

            return Ok(new { Message = "User registered successfully" });
        }

        // ======== Логин ========
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return Unauthorized("Invalid email or password.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password.");

            var token = GenerateJwtToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                Email = user.Email,
                Role = user.Role.ToString(),
                UserId = user.Id
            });
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
