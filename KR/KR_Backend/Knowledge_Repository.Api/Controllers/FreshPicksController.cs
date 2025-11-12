using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Controllers
{
 
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
            var picks = await _freshPickService.GetFreshPicksAsync(count);
            return Ok(picks);
        }
    }
}
