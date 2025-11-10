using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities; // For Domain and Category entities
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DomainsController : ControllerBase
    {
        private readonly IDomainService _domainService;

        public DomainsController(IDomainService domainService)
        {
            _domainService = domainService;
        }

        /// <summary>
        /// Retrieves all domains.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Domains>>> GetAllDomains()
        {
            var domains = await _domainService.GetAllDomainsAsync();
            return Ok(domains);
        }

        /// <summary>
        /// Retrieves a domain by its ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Domains>> GetDomainById(Guid id)
        {
            var domain = await _domainService.GetDomainByIdAsync(id);
            if (domain == null)
                return NotFound();
            return Ok(domain);
        }

        /// <summary>
        /// Retrieves a domain by its name.
        /// </summary>
        [HttpGet("byname/{name}")]
        public async Task<ActionResult<Domains>> GetDomainByName(string name)
        {
            var domain = await _domainService.GetDomainByNameAsync(name);
            if (domain == null)
                return NotFound();
            return Ok(domain);
        }

        /// <summary>
        /// Retrieves all categories for a given domain ID.
        /// </summary>
        [HttpGet("{domainid}/categories")]
        public async Task<ActionResult<List<Category>>> GetCategoriesByDomainId(Guid domainid)
        {
            var categories = await _domainService.GetCategoriesByDomainIdAsync(domainid);
            if (categories == null || !categories.Any())
                return NotFound();
            return Ok(categories);
        }
        [HttpPost]
        public async Task<ActionResult> CreateDomain([FromBody] Domains domain)
        {
            if (string.IsNullOrWhiteSpace(domain.DomainName))
                return BadRequest("Domain name is required.");

            await _domainService.AddDomainAsync(domain);
            return Ok(new { message = "Domain added successfully." });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateDomain(Guid id, [FromBody] Domains domain)
        {
            var success = await _domainService.UpdateDomainAsync(id, domain);
            if (!success) return NotFound();
            return Ok(new { message = "Domain updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDomain(Guid id)
        {
            var success = await _domainService.DeleteDomainAsync(id);
            if (!success) return NotFound();
            return Ok(new { message = "Domain deleted successfully." });
        }

    }
}
