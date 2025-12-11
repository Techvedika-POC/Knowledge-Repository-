using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResourceController : ControllerBase
    {
        private readonly IResourceService _resourceService;

        public ResourceController(IResourceService resourceService)
        {
            _resourceService = resourceService;
        }

        [HttpGet("{resourceId}")]
        public async Task<ActionResult<ResourceDto>> GetResource(Guid resourceId)
        {
            var resource = await _resourceService.GetResourceByIdAsync(resourceId);
            if (resource == null) return NotFound();
            return Ok(resource);
        }

        [HttpGet("topic/{topicId}")]
        public async Task<ActionResult<IEnumerable<ResourceDto>>> GetResourcesByTopic(Guid topicId, [FromQuery] Guid? moduleId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var resources = await _resourceService.GetResourcesByTopicOrModuleAsync(topicId, moduleId, page, pageSize);
            return Ok(resources);
        }

        [HttpPost]
        public async Task<ActionResult<ResourceDto>> CreateResource([FromBody] ResourceDto resourceDto)
        {
            var created = await _resourceService.CreateResourceAsync(resourceDto);
            return Ok(created);
        }

        [HttpPost("batch")]
        public async Task<ActionResult<IEnumerable<ResourceDto>>> CreateResourcesBatch([FromBody] IEnumerable<ResourceDto> resources)
        {
            var created = await _resourceService.CreateResourcesBatchAsync(resources);
            return Ok(created);
        }

        [HttpPut("{resourceId}")]
        public async Task<IActionResult> UpdateResource(Guid resourceId, [FromBody] ResourceDto resourceDto)
        {
            if (resourceId != resourceDto.ResourceId) return BadRequest("Resource ID mismatch.");
            await _resourceService.UpdateResourceAsync(resourceDto);
            return NoContent();
        }

        [HttpPut("batch")]
        public async Task<IActionResult> UpdateResourcesBatch([FromBody] IEnumerable<ResourceDto> resources)
        {
            await _resourceService.UpdateResourcesBatchAsync(resources);
            return NoContent();
        }

        [HttpDelete("{resourceId}")]
        public async Task<IActionResult> DeleteResource(Guid resourceId)
        {
            await _resourceService.DeleteResourceAsync(resourceId);
            return NoContent();
        }

        [HttpPost("{resourceId}/access/{userId}")]
        public async Task<IActionResult> MarkAccessed(Guid resourceId, Guid userId)
        {
            await _resourceService.MarkResourceAccessedAsync(resourceId, userId);
            return NoContent();
        }
    }
}
