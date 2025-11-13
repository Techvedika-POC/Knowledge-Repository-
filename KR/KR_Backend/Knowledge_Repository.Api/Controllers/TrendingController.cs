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
            var result = await _trendingService.GetTrendingAsync(top);
            return Ok(result);
        }
    }
}
