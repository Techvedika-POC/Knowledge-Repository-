using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;
using KnowledgeSynaptix.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Controllers
{
    /// <summary>
    /// Handles generation of text embeddings through the IEmbeddingService.
    /// Embeddings are numerical representations of text used in AI and NLP tasks.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EmbeddingsController : ControllerBase
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly ILogger<EmbeddingsController> _logger;

        public EmbeddingsController(IEmbeddingService embeddingService, ILogger<EmbeddingsController> logger)
        {
            _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates an embedding (numerical representation) for the provided text.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateEmbedding([FromBody] EmbeddingRequest request)
        {
            // Validate request
            if (request == null || string.IsNullOrWhiteSpace(request.Text))
            {
                _logger.LogWarning("Received empty or null text in embedding request.");
                return BadRequest("Text is required.");
            }

            _logger.LogInformation("Generating embedding for text: {Text}", request.Text);

            // Generate embedding vector using the embedding service
            var embedding = await _embeddingService.GetEmbeddingAsync(request.Text);

            // Check if embedding generation failed
            if (embedding == null || embedding.Length == 0)
            {
                _logger.LogError("Failed to generate embedding for text: {Text}", request.Text);
                return StatusCode(500, "Failed to generate embedding.");
            }

            _logger.LogInformation("Embedding generated successfully. Length: {Length}", embedding.Length);

            // Return embedding as JSON response
            return Ok(new { embedding });
        }
    }

    /// <summary>
    /// Represents the input model for creating embeddings.
    /// </summary>
    public class EmbeddingRequest
    {
        public string Text { get; set; } = string.Empty; // Required text input
    }
}
