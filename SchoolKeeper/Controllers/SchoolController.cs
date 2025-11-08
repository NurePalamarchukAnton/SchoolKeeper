using Microsoft.AspNetCore.Mvc;
using SchoolKeeper.Abstractions.Interfaces.Repository;
using SchoolKeeper.DTO;

namespace SchoolKeeper.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchoolController : BaseController<School, SchoolDto>
    {
        public SchoolController(IGenericRepository<School> repo) : base(repo) { }
    }

}
