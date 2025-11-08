using Microsoft.AspNetCore.Mvc;
using SchoolKeeper.Abstractions.Interfaces.Repository;
using SchoolKeeper.DTO;

namespace SchoolKeeper.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : BaseController<Device, DeviceDto>
    {
        public DeviceController(IGenericRepository<Device> repo) : base(repo) { }
    }

}
