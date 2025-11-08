using Microsoft.AspNetCore.Mvc;
using SchoolKeeper.Abstractions.Interfaces.Repository;
using SchoolKeeper.DTO;

namespace SchoolKeeper.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ReptController : BaseController<Rept, ReptDto>
    {
        public ReptController(IGenericRepository<Rept> repo) : base(repo) { }
    }

}
