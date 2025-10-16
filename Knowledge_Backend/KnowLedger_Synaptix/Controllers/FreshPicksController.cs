using Microsoft.AspNetCore.Mvc;
using KnowLedger_Synaptix.Services.Interfaces;
using KnowLedger_Synaptix.Dtos;

namespace KnowLedger_Synaptix.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FreshPicksController : ControllerBase
    {
        private readonly IFreshPickService _freshPickService;

        public FreshPicksController(IFreshPickService freshPickService)
        {
            _freshPickService = freshPickService;
        }

        [HttpGet]
        public async Task<ActionResult<List<KnowledgeItemFilterDto>>> GetFreshPicks([FromQuery] int count = 10)
        {
            var picks = await _freshPickService.GetFreshPicksAsync(count);
            return Ok(picks);
        }
    }
}
