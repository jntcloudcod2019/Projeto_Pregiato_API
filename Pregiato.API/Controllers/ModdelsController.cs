using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Pregiato.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Models")]
    public class ModdelsController : ControllerBase
    {
        
    }
}
