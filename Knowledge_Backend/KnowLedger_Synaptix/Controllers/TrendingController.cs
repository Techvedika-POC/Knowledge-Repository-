using KnowLedger_Synaptix.Services.Interfaces;
using KnowLedger_Synaptix.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Controllers
{
    /// <summary>
    /// Provides endpoints to retrieve trending knowledge items based on engagement.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TrendingController : ControllerBase
    {
        private readonly ITrendingService _trendingService;

        public TrendingController(ITrendingService trendingService)
        {
            _trendingService = trendingService ?? throw new ArgumentNullException(nameof(trendingService));
        }

        [HttpGet]
        public async Task<ActionResult<List<KnowledgeItemDto>>> GetTrending([FromQuery] int top = 5)
        {
            // Fetch top trending knowledge items based on engagement score
            var result = await _trendingService.GetTrendingAsync(top);
            return Ok(result);
        }
    }
}
