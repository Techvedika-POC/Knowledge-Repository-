using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Knowledge_Repository.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DaySpotlightController : ControllerBase
    {
        private readonly IDaySpotlightService _spotlightService;

        public DaySpotlightController(IDaySpotlightService spotlightService)
        {
            _spotlightService = spotlightService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDaySpotlight()
        {
            var result = await _spotlightService.GetDaySpotlightAsync();
            return Ok(result);
        }
    }
}
