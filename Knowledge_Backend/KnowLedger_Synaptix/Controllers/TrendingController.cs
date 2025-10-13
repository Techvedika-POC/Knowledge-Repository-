using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrendingController : ControllerBase
    {
        private readonly ITrendingService _trendingService;

        public TrendingController(ITrendingService trendingService)
        {
            _trendingService = trendingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrending([FromQuery] int top = 5)
        {
            var result = await _trendingService.GetTrendingAsync(top);
            return Ok(result);
        }
    }
}
