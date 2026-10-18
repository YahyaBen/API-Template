using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodiya.API.Controllers.Common
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
    }
}
