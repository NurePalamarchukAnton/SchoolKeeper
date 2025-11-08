using Microsoft.AspNetCore.Mvc;
using SchoolKeeper.Abstractions.Interfaces.Repository;
using SchoolKeeper.DTO;

namespace SchoolKeeper.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : BaseController<User, UserDto>
    {
        public UserController(IGenericRepository<User> repo) : base(repo) { }
    }

}
