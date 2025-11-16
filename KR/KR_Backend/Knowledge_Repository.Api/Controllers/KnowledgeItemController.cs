using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

            if (Files != null)
            {
                var uploadRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
                if (!Directory.Exists(uploadRoot))
                    Directory.CreateDirectory(uploadRoot);

                foreach (var file in Files)
                {
                    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var filePath = Path.Combine(uploadRoot, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    byte[] fileBytes;
                    using (var ms = new MemoryStream())
                    {
                        await file.CopyToAsync(ms);
                        fileBytes = ms.ToArray();
                    }

                    request.Attachments.Add(new FileAttachmentDto
                    {
                        FileName = file.FileName,
                        MimeType = file.ContentType,
                        FilePath = $"/uploads/{fileName}",
                        FileSize = file.Length,
                        FileData = fileBytes
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

            // Build attachments list from incoming IFormFile objects (mirrors your Upload flow)
            dto.Attachments = dto.Attachments ?? new List<FileAttachmentDto>();

            if (Files != null && Files.Count > 0)
            {
                var uploadRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
                if (!Directory.Exists(uploadRoot))
                    Directory.CreateDirectory(uploadRoot);

                foreach (var file in Files)
                {
                    // Persist the file to wwwroot/uploads (same as Upload)
                    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var filePath = Path.Combine(uploadRoot, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Read bytes into memory to pass to service as FileAttachmentDto.FileData
                    byte[] fileBytes;
                    using (var ms = new MemoryStream())
                    {
                        await file.CopyToAsync(ms);
                        fileBytes = ms.ToArray();
                    }

                    dto.Attachments.Add(new FileAttachmentDto
                    {
                        FileName = file.FileName,
                        MimeType = file.ContentType,
                        FilePath = $"/uploads/{fileName}",   // optional, helpful for preview
                        FileSize = file.Length,
                        FileData = fileBytes
                    });
                }
            }

            // Get caller user id
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User not logged in.");

            var userId = Guid.Parse(userIdClaim);

            // Call the service (service expects dto.Attachments and ReplaceAttachments flag)
            try
            {
                var updated = await _service.UpdateKnowledgeItemAsync(itemId, dto, userId);
                return Ok(new { success = true, item = updated });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Log if you have logger; keep response generic
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, error = ex.Message });
            }
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
            var result = await _service.GetVersionsWithFilesAsync(itemId);
            return Ok(result);
        }
    }
}
