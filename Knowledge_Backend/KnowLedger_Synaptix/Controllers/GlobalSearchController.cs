using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GlobalSearchController : ControllerBase
    {
        private readonly IGlobalSearchService _globalSearchService;

        public GlobalSearchController(IGlobalSearchService globalSearchService)
        {
            _globalSearchService = globalSearchService;
        }

        // GET: api/GlobalSearch?keyword=abc
        [HttpGet]
        public async Task<ActionResult<List<GlobalSearchResultDto>>> Get([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest("Keyword cannot be empty.");

            try
            {
                var results = await _globalSearchService.GlobalSearchAsync(keyword);

                if (results == null || results.Count == 0)
                    return NotFound("No matching results found.");

                return Ok(results);
            }
            catch (Exception ex)
            {
                // Optionally log the exception
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
