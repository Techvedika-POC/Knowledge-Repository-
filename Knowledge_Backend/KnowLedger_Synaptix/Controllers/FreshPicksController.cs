using Microsoft.AspNetCore.Mvc;
using KnowLedger_Synaptix.Services.Interfaces;
using KnowLedger_Synaptix.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Controllers
{
    /// <summary>
    /// Retrieves recently created or trending knowledge items (Fresh Picks).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FreshPicksController : ControllerBase
    {
        private readonly IFreshPickService _freshPickService;

        public FreshPicksController(IFreshPickService freshPickService)
        {
            _freshPickService = freshPickService ?? throw new ArgumentNullException(nameof(freshPickService));
        }

        [HttpGet]
        public async Task<ActionResult<List<KnowledgeItemDto>>> GetFreshPicks([FromQuery] int count = 10)
        {
            // Fetch the most recent knowledge items from the service
            var picks = await _freshPickService.GetFreshPicksAsync(count);
            return Ok(picks);
        }
    }
}
