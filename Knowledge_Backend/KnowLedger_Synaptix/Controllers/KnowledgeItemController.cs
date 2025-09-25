using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Services;
using KnowLedger_Synaptix.Models;
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
        public async Task<IActionResult> Upload(
            [FromForm] KnowledgeItemUploadDto request,
            [FromForm] List<IFormFile> Files
        )
        {
            //Map uploaded files into DTO.Attachments
            request.Attachments = new List<FileAttachmentDto>();
            if (Files != null)
            {
                foreach (var file in Files)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    request.Attachments.Add(new FileAttachmentDto
                    {
                        FileName = file.FileName,
                        MimeType = file.ContentType,
                        FileData = ms.ToArray(),
                        FileSize = file.Length
                    });
                }
            }

            //  Replace with actual logged-in user later
            var userId = Guid.Parse("ea7f7e02-09f0-4ab6-8e20-70f613fbb7bd");

            // Call service with dto (already contains IsEventItem, EventId, TeamMemberEmails)
            var item = await _service.UploadKnowledgeItemAsync(request, userId);

            return Ok(new { success = true, itemId = item.ItemId });
        }
    }
}
