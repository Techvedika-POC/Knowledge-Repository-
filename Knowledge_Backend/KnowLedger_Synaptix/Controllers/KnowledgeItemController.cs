using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KnowledgeItemController : ControllerBase
    {
        private readonly IKnowledgeItemService _service;

        public KnowledgeItemController(IKnowledgeItemService service)
        {
            _service = service;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] KnowledgeItemUploadDto request, [FromForm] List<IFormFile> files)
        {
            var attachments = new List<AttachmentDto>();

            if (files != null && files.Count > 0)
            {
                request.Attachments = new List<AttachmentDto>();
                foreach (var file in files)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    request.Attachments.Add(new AttachmentDto
                    {
                        FileName = file.FileName,
                        MimeType = file.ContentType,
                        FileData = ms.ToArray(),
                        FileSize = file.Length
                    });
                }
            }
            var userId = Guid.Parse("9f223c35-bb26-4fd9-91f6-3ab0aaaf3e92");

            var item = await _service.UploadKnowledgeItemAsync(request, userId);

            return Ok(new { success = true, itemId = item.ItemId });
        }

        [HttpGet("domains")]
        public async Task<IActionResult> GetDomains()
        {
            var domains = await _service.GetAllDomainsAsync();
            return Ok(domains);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories([FromQuery] Guid domainId)
        {
            var categories = await _service.GetCategoriesByDomainAsync(domainId);
            return Ok(categories);
        }

        [HttpGet("events")]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _service.GetAllEventsAsync();
            return Ok(events);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }
    }
}
