using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Controllers
{
    /// <summary>
    /// Provides a global search endpoint to query knowledge items by keyword.
    /// </summary>
    [ApiController]
    [Route("api/global-search")]
    [Route("api/GlobalSearch")] // Use lowercase URL for consistency
    public class GlobalSearchController : ControllerBase
    {
        private readonly IGlobalSearchService _globalSearchService;

        public GlobalSearchController(IGlobalSearchService globalSearchService)
        {
            _globalSearchService = globalSearchService;
        }

        /// <summary>
        /// Performs a global search on knowledge items by keyword.
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(new { message = "Keyword cannot be empty." });

            try
            {
                var results = await _globalSearchService.GlobalSearchAsync(keyword);

                if (results == null || results.Count == 0)
                    return NotFound(new { message = "No matching results found." });

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}
