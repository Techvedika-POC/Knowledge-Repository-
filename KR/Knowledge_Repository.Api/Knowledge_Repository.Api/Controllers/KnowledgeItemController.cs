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
    /// <summary>
    /// Controller for managing knowledge items including upload, retrieval, 
    /// and filtering by domain, category, or all items.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Only authenticated users can perform uploads
    public class KnowledgeItemController : ControllerBase
    {
        private readonly IKnowledgeItemService _service;
        private readonly IWebHostEnvironment _env;

        public KnowledgeItemController(IKnowledgeItemService service, IWebHostEnvironment env)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        // -----------------------------
        // UPLOAD ITEM
        // -----------------------------
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

        // -----------------------------
        // GET ITEM DETAILS
        // -----------------------------
        [HttpGet("{itemId}/details")]
        public async Task<ActionResult<KnowledgeItemDetailsDto>> GetKnowledgeItemDetails(Guid itemId)
        {
            var details = await _service.GetKnowledgeItemDetailsAsync(itemId);
            if (details == null) return NotFound();
            return Ok(details);
        }

        // -----------------------------
        // GET ALL ITEMS
        // -----------------------------
        [HttpGet("All")]
        [AllowAnonymous] // Optional: allow public access
        public async Task<IActionResult> GetAllItems()
        {
            var items = await _service.GetKnowledgeItemsAsync();
            return Ok(items);
        }

        // -----------------------------
        // GET ITEMS BY DOMAIN
        // -----------------------------
        [HttpGet("ByDomain/{domainId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByDomain(Guid domainId)
        {
            var items = await _service.GetKnowledgeItemsAsync(domainId, null);
            return Ok(items);
        }

        // -----------------------------
        // GET ITEMS BY CATEGORY
        // -----------------------------
        [HttpGet("ByCategory/{categoryId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategory(Guid categoryId)
        {
            var items = await _service.GetKnowledgeItemsAsync(null, categoryId);
            return Ok(items);
        }
    }
}
