using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Implementations;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class KnowledgeItemController : ControllerBase
    {
        private readonly IKnowledgeItemService _service;
        [HttpGet("{itemId}/details")]
        public async Task<ActionResult<KnowledgeItemDetailsDto>> GetKnowledgeItemDetails(Guid itemId)
        {
            var details = await _service.GetKnowledgeItemDetailsAsync(itemId);
            if (details == null) return NotFound();
            return Ok(details);
        }

        public KnowledgeItemController(IKnowledgeItemService service)
        {
            _service = service;
        }
        [AllowAnonymous]
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

            // Get logged-in user ID from JWT token claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User not logged in.");

            var userId = Guid.Parse(userIdClaim);

            // Call service with DTO (already contains IsEventItem, EventId, TeamMemberEmails)
            var item = await _service.UploadKnowledgeItemAsync(request, userId);

            return Ok(new { success = true, itemId = item.ItemId });
        }



        [AllowAnonymous]

        [HttpGet("Datewise")]
        public async Task<ActionResult<IEnumerable<KnowledgeItemFilterDto>>> GetKnowledgeItemSummaries(
      [FromQuery] string sortOrder = "desc",
      [FromQuery] DateTime? filterDate = null
  )
        {
            var result = await _service.GetKnowledgeItemSummariesAsync(sortOrder, filterDate);
            return Ok(result);
        }
        [AllowAnonymous]
        [HttpGet("ByDomain/{domainId}")]
        public async Task<ActionResult<IEnumerable<KnowledgeItemFilterDto>>> GetByDomain(Guid domainId)
        {
            var result = await _service.GetKnowledgeItemsByDomainAsync(domainId);
            return Ok(result);
        }
        [AllowAnonymous]
        [HttpGet("ByCategory/{categoryId}")]
        public async Task<ActionResult<IEnumerable<KnowledgeItemFilterDto>>> GetByCategory(Guid categoryId)
        {
            var result = await _service.GetKnowledgeItemsByCategoryAsync(categoryId);
            return Ok(result);
        }
        [HttpGet("All")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<KnowledgeItemFilterDto>>> GetAllKnowledgeItems()
        {
            var result = await _service.GetAllKnowledgeItemsAsync();
            return Ok(result);
        }


    }
}
