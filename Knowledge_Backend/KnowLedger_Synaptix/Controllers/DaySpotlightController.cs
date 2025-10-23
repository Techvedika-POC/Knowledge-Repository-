using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Controllers
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
        //Getting the items to disply in spotlight 
        [HttpGet]
        public async Task<IActionResult> GetDaySpotlight()
        {
            var result = await _spotlightService.GetDaySpotlightAsync();
            return Ok(result);
        }
    }
}
