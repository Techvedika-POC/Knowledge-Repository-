using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowLedger_Synaptix.Controllers
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

        [HttpGet]
        public async Task<ActionResult<List<DomainDto>>> GetAllDomains()
        {
            var domains = await _domainService.GetAllDomainsAsync();
            return Ok(domains);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DomainDto>> GetDomainById(Guid id)
        {
            var domain = await _domainService.GetDomainByIdAsync(id);
            if (domain == null)
            {
                return NotFound();
            }
            return Ok(domain);
        }
        [HttpGet("byname/{name}")]
        public async Task<ActionResult<DomainDto>> GetDomainByName(string name)
        {
            var domain = await _domainService.GetDomainByNameAsync(name);
            if (domain == null)
                return NotFound();
            return Ok(domain);
        }
        [HttpGet("{domainid}/categories")]
        public async Task<ActionResult<List<CategoryDto>>> GetCategoriesByDomainId(Guid domainid)
        {
            var categories = await _domainService.GetCategoriesByDomainIdAsync(domainid);
            if (categories == null || !categories.Any())
                return NotFound();
            return Ok(categories);
        }



    }
}
