using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Controllers
{
    /// <summary>
    /// Provides a global search endpoint to query knowledge items by keyword.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GlobalSearchController : ControllerBase
    {
        private readonly IGlobalSearchService _globalSearchService;

        public GlobalSearchController(IGlobalSearchService globalSearchService)
        {
            _globalSearchService = globalSearchService;
        }

        [HttpGet]
        public async Task<ActionResult<List<KnowledgeItemDto>>> Get([FromQuery] string keyword)
        {
            // Validate the input keyword
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest("Keyword cannot be empty.");

            try
            {
                // Perform global search using the service
                var results = await _globalSearchService.GlobalSearchAsync(keyword);

                // Check if no results found
                if (results == null || results.Count == 0)
                    return NotFound("No matching results found.");

                return Ok(results);
            }
            catch (Exception ex)
            {
                // Return internal server error with exception message
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
