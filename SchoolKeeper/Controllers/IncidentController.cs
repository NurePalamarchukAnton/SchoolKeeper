using Microsoft.AspNetCore.Mvc;
using SchoolKeeper.Abstractions.Interfaces.Repository;
using SchoolKeeper.DTO;

namespace SchoolKeeper.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IncidentController : BaseController<Incident, IncidentDto>
    {
        public IncidentController(IGenericRepository<Incident> repo) : base(repo) { }
    }

}
