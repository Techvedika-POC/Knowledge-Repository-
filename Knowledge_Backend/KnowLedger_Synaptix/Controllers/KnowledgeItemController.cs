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
    /// <summary>
    /// Controller for managing knowledge items including upload, retrieval, 
    /// and filtering by date, domain, or category.
    /// </summary>
    [Route("api/[controller]")]
    [Authorize] // Only authenticated users can perform uploads
    [ApiController]
    public class KnowledgeItemController : ControllerBase
    {
        private readonly IKnowledgeItemService _service;
        private readonly IWebHostEnvironment _env;

        public KnowledgeItemController(IKnowledgeItemService service, IWebHostEnvironment env)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        /// <summary>
        /// Uploads a new knowledge item with optional file attachments.
        /// Saves files to the server and returns the created item's ID.
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(
            [FromForm] KnowledgeItemUploadDto request,
            [FromForm] List<IFormFile> Files)
        {
            //Map uploaded files into DTO.Attachments
            request.Attachments = new List<FileAttachmentDto>();

            if (Files != null)
            {
                // Determine server upload directory
                var uploadRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
                if (!Directory.Exists(uploadRoot))
                    Directory.CreateDirectory(uploadRoot);

                foreach (var file in Files)
                {
                    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var filePath = Path.Combine(uploadRoot, fileName);

                    // Save file to server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Add file metadata to request object
                    request.Attachments.Add(new FileAttachmentDto
                    {
                        FileName = file.FileName,
                        MimeType = file.ContentType,
                        FilePath = $"/uploads/{fileName}", 
                        FileSize = file.Length,
                        FileData = null 
                    });
                }
            }

            // Get the logged-in user ID from JWT claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User not logged in.");

            var userId = Guid.Parse(userIdClaim);

            // Upload knowledge item via service
            var item = await _service.UploadKnowledgeItemAsync(request, userId);

            return Ok(new { success = true, itemId = item.ItemId });
        }

        /// <summary>
        /// Retrieves detailed information for a specific knowledge item.
        /// </summary>
        
        [HttpGet("{itemId}/details")]
        public async Task<ActionResult<KnowledgeItemDetailsDto>> GetKnowledgeItemDetails(Guid itemId)
        {
            var details = await _service.GetKnowledgeItemDetailsAsync(itemId);
            if (details == null) return NotFound(); 
            return Ok(details);
        }

        /// <summary>
        /// Retrieves knowledge item summaries optionally filtered by date
        /// and sorted by ascending or descending order.
        /// </summary>
   
        [HttpGet("Datewise")]
        public async Task<ActionResult<IEnumerable<KnowledgeItemFilterDto>>> GetKnowledgeItemSummaries(
            [FromQuery] string sortOrder = "desc",
            [FromQuery] DateTime? filterDate = null)
        {
            // Fetch summaries from service
            var result = await _service.GetKnowledgeItemSummariesAsync(sortOrder, filterDate);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves knowledge items filtered by domain ID.
        /// </summary>
     
        [HttpGet("ByDomain/{domainId}")]
        public async Task<ActionResult<IEnumerable<KnowledgeItemFilterDto>>> GetByDomain(Guid domainId)
        {
            var result = await _service.GetKnowledgeItemsByDomainAsync(domainId);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves knowledge items filtered by category ID.
        /// </summary>
      
        [HttpGet("ByCategory/{categoryId}")]
        public async Task<ActionResult<IEnumerable<KnowledgeItemFilterDto>>> GetByCategory(Guid categoryId)
        {
            // Fetch category-specific knowledge items
            var result = await _service.GetKnowledgeItemsByCategoryAsync(categoryId);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all knowledge items.
        /// </summary>
    
        [HttpGet("All")]
        public async Task<ActionResult<IEnumerable<KnowledgeItemFilterDto>>> GetAllKnowledgeItems()
        {
            // Fetch all knowledge items from the service
            var result = await _service.GetAllKnowledgeItemsAsync();
            return Ok(result);
        }


    }
}
