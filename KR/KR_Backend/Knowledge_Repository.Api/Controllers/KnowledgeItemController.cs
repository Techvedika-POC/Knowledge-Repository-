using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Implementations.Services;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Knowledge_Repository.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class KnowledgeItemController : ControllerBase
    {
        private readonly IKnowledgeItemService _service;
        private readonly IWebHostEnvironment _env;

        public KnowledgeItemController(IKnowledgeItemService service, IWebHostEnvironment env)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(
       [FromForm] KnowledgeItemUploadDto request,
       [FromForm] List<IFormFile> Files)
        {
            request.Attachments = new List<FileAttachmentDto>();

            if (Files != null && Files.Count > 0)
            {
                foreach (var file in Files)
                {
                    if (file == null || file.Length == 0)
                        continue;

                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);

                    request.Attachments.Add(new FileAttachmentDto
                    {
                        FileName = file.FileName,
                        MimeType = file.ContentType,
                        FileSize = file.Length,
                        FileData = ms.ToArray()
                    });
                }
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User not logged in.");

            var userId = Guid.Parse(userIdClaim);
            var item = await _service.UploadKnowledgeItemAsync(request, userId);

            return Ok(new { success = true, itemId = item.ItemId });
        }

        [HttpPut]
        public async Task<IActionResult> Update(
    Guid itemId,
    [FromForm] KnowledgeItemUpdateDto dto,
    [FromForm] List<IFormFile>? Files)
        {
            if (dto == null) return BadRequest("Update payload is required.");

            dto.Attachments = dto.Attachments ?? new List<FileAttachmentDto>();

            if (Files != null && Files.Count > 0)
            {
                foreach (var file in Files)
                {
                    if (file == null || file.Length == 0)
                        continue;

                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);

                    dto.Attachments.Add(new FileAttachmentDto
                    {
                        FileName = file.FileName,
                        MimeType = file.ContentType,
                        FileSize = file.Length,
                        FileData = ms.ToArray()
                    });
                }
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User not logged in.");

            var userId = Guid.Parse(userIdClaim);

            var updated = await _service.UpdateKnowledgeItemAsync(itemId, dto, userId);
            return Ok(new { success = true, item = updated });
        }

        [HttpGet("{itemId}/details")]
        public async Task<ActionResult<KnowledgeItemDetailsDto>> GetKnowledgeItemDetails(Guid itemId)
        {
            var details = await _service.GetKnowledgeItemDetailsAsync(itemId);
            if (details == null) return NotFound();
            return Ok(details);
        }

        [HttpGet("All")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllItems()
        {
            var items = await _service.GetKnowledgeItemsAsync();
            return Ok(items);
        }

        [HttpGet("ByDomain/{domainId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByDomain(Guid domainId)
        {
            var items = await _service.GetKnowledgeItemsAsync(domainId, null);
            return Ok(items);
        }


        [HttpGet("ByCategory/{categoryId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategory(Guid categoryId)
        {
            var items = await _service.GetKnowledgeItemsAsync(null, categoryId);
            return Ok(items);
        }
       
        [HttpGet("{itemId}/versions")]
        public async Task<IActionResult> GetVersionsWithFiles(Guid itemId)
        {
            var versions = await _service.GetVersionsWithFilesAsync(itemId);
            if (versions == null) return Ok(new List<VersionWithAttachmentsDto>());
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            foreach (var v in versions)
            {
                if (v.Attachments == null) continue;
                foreach (var a in v.Attachments)
                {

                    if (!string.IsNullOrWhiteSpace(a.FileUrl) && (a.FileUrl.StartsWith("http://") || a.FileUrl.StartsWith("https://")))
                        continue;

                 
                    var rel = a.FilePath ?? a.FileUrl;
                    if (string.IsNullOrWhiteSpace(rel)) continue;

                    if (!rel.StartsWith("/")) rel = "/" + rel.TrimStart('/');

                    a.FileUrl = baseUrl + rel;
                }
            }

            return Ok(versions);
        }

        [HttpGet("attachment/{attachmentId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAttachment(Guid attachmentId)
        {
            var att = await _service.GetAttachmentByIdAsync(attachmentId);
            if (att == null)
                return NotFound();
            string? physicalPath = null;
            if (!string.IsNullOrWhiteSpace(att.FilePath))
            {
                var trimmed = att.FilePath.TrimStart('/', '\\');
                physicalPath = Path.Combine(_env.WebRootPath ?? "wwwroot", trimmed.Replace('/', Path.DirectorySeparatorChar));
            }

            if (string.IsNullOrWhiteSpace(physicalPath) || !System.IO.File.Exists(physicalPath))
            {
                return NotFound("File missing on server.");
            }


            var provider = new FileExtensionContentTypeProvider();
            string contentType;
            if (!provider.TryGetContentType(att.FileName ?? physicalPath, out contentType))
                contentType = "application/octet-stream";

           
            Response.Headers["Content-Disposition"] = $"inline; filename=\"{att.FileName}\"";
            Response.Headers["Accept-Ranges"] = "bytes";

            var fs = System.IO.File.OpenRead(physicalPath);

            return new FileStreamResult(fs, contentType)
            {
                EnableRangeProcessing = true
            };
        }
        [AllowAnonymous]
        [HttpGet("event-items/approved")]
        public async Task<IActionResult> GetApprovedEventItems()
        {
            var result = await _service.GetApprovedEventItemsAsync();
            return Ok(result);
        }




    }
}

