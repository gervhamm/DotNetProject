using Microsoft.AspNetCore.Mvc;

namespace ArcsomAssetManagement.Api.Controllers
{
  
    [ApiController]
    [Route("api/[controller]")]
    public class AssetController : ControllerBase
    {
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
