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
    public class DeviceController : BaseController<Device, DeviceDto>
    {
        private readonly IUserService _userService;
        private readonly IDataFilterService _dataFilterService;
        private readonly SchoolKeeperDbContext _context;

        public DeviceController(
            IGenericRepository<Device> repo,
            IUserService userService,
            IDataFilterService dataFilterService,
            SchoolKeeperDbContext context) 
            : base(repo)
        {
            _userService = userService;
            _dataFilterService = dataFilterService;
            _context = context;
        }

        public override async Task<ActionResult<ResponseWrapper<IEnumerable<DeviceDto>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext);
            if (user == null) throw new UnauthorizedException();

            // Согласно матрице: только Admin и Security
            if (user.Role != UserRole.Admin && user.Role != UserRole.Security)
            {
                throw new UnauthorizedException("You don't have access to devices.");
            }

            var query = _context.Devices.AsQueryable();
            
            // Filter by school (except Admin)
            if (user.Role != UserRole.Admin)
            {
                query = _dataFilterService.FilterBySchool(query, user.SchoolId);
            }

            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 500) pageSize = 50;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Select(MapToDto).ToList();
            var response = new ResponseWrapper<IEnumerable<DeviceDto>>(200, dtos);
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Policy = Policies.AdminOnly)]
        public override async Task<ActionResult<ResponseWrapper<DeviceDto>>> Create([FromBody] DeviceDto dto)
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext);
            if (user == null) throw new UnauthorizedException();

            // Non-admin users can only create devices for their school
            if (user.Role != UserRole.Admin && dto.SchoolId != user.SchoolId)
            {
                throw new BadRequestException("You can only create devices for your school.");
            }

            return await base.Create(dto);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public override async Task<ActionResult<ResponseWrapper<DeviceDto>>> Update(int id, [FromBody] DeviceDto dto)
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext);
            if (user == null) throw new UnauthorizedException();

            var device = await Repo.GetByIdAsync(id);
            if (device == null) throw new NotFoundException();

            // Check school access
            if (user.Role != UserRole.Admin && device.SchoolId != user.SchoolId)
            {
                throw new UnauthorizedException("You don't have access to this device.");
            }

            return await base.Update(id, dto);
        }

        [HttpGet("{id:int}")]
        public override async Task<ActionResult<ResponseWrapper<DeviceDto>>> GetById(int id)
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext);
            if (user == null) throw new UnauthorizedException();

            // Согласно матрице: только Admin и Security
            if (user.Role != UserRole.Admin && user.Role != UserRole.Security)
            {
                throw new UnauthorizedException("You don't have access to devices.");
            }

            var device = await Repo.GetByIdAsync(id);
            if (device == null) throw new NotFoundException();

            // Check school access
            if (user.Role != UserRole.Admin && device.SchoolId != user.SchoolId)
            {
                throw new UnauthorizedException("You don't have access to this device.");
            }

            var dto = MapToDto(device);
            var response = new ResponseWrapper<DeviceDto>(200, dto);
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
