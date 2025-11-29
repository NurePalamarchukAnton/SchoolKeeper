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
    public class SchoolController : BaseController<School, SchoolDto>
    {
        private readonly IUserService _userService;
        private readonly SchoolKeeperDbContext _context;

        public SchoolController(
            IGenericRepository<School> repo,
            IUserService userService,
            SchoolKeeperDbContext context) 
            : base(repo)
        {
            _userService = userService;
            _context = context;
        }

        public override async Task<ActionResult<ResponseWrapper<IEnumerable<SchoolDto>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext);
            if (user == null) throw new UnauthorizedException();

            IQueryable<School> query = _context.Schools.AsQueryable();

            // Non-admin users can only see their own school
            if (user.Role != UserRole.Admin)
            {
                query = query.Where(s => s.Id == user.SchoolId);
            }

            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 500) pageSize = 50;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Select(MapToDto).ToList();
            var response = new ResponseWrapper<IEnumerable<SchoolDto>>(200, dtos);
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Policy = Policies.AdminOnly)]
        public override async Task<ActionResult<ResponseWrapper<SchoolDto>>> Create([FromBody] SchoolDto dto)
        {
            return await base.Create(dto);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public override async Task<ActionResult<ResponseWrapper<SchoolDto>>> Update(int id, [FromBody] SchoolDto dto)
        {
            return await base.Update(id, dto);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public override async Task<ActionResult<ResponseWrapper<object>>> Delete(int id)
        {
            return await base.Delete(id);
        }
    }
}
