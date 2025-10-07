using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KnowLedger_Synaptix.Services.Interfaces;

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
            var userId = Guid.Parse("9f223c35-bb26-4fd9-91f6-3ab0aaaf3e92");

            // Call service with dto (already contains IsEventItem, EventId, TeamMemberEmails)
            var item = await _service.UploadKnowledgeItemAsync(request, userId);

            return Ok(new { success = true, itemId = item.ItemId });
        }
    }
}
