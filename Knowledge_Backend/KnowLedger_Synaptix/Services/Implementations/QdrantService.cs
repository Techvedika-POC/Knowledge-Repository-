using KnowledgeSynaptix.Services.Interfaces;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KnowledgeSynaptix.Services.Implementations
{
    /// <summary>
    /// Service for interacting with Qdrant vector database to store embeddings.
    /// </summary>
    public class QdrantService : IQdrantService
    {
        private readonly HttpClient _client;
        private const string QdrantUrl = "http://localhost:6333"; // Base URL of Qdrant service
        private readonly int _expectedDim;

        /// <summary>
        /// Initializes a new instance of the <see cref="QdrantService"/> class.
        /// </summary>
        /// <param name="client">Injected HttpClient for making requests</param>
        /// <param name="expectedDim">Expected dimensionality of the embeddings</param>
        public QdrantService(HttpClient client, int expectedDim = 384)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _expectedDim = expectedDim;
        }

        /// <summary>
        /// Saves a vector embedding along with metadata to Qdrant.
        /// </summary>
        /// <param name="id">Unique identifier of the item</param>
        /// <param name="embedding">Vector embedding (must match expected dimension)</param>
        /// <param name="title">Title of the knowledge item</param>
        /// <param name="description">Description of the knowledge item</param>
        /// <exception cref="ArgumentNullException">Thrown if embedding is null</exception>
        /// <exception cref="ArgumentException">Thrown if embedding dimension is incorrect</exception>
        /// <exception cref="InvalidOperationException">Thrown if Qdrant request fails</exception>
        public async Task SaveToQdrantAsync(string id, float[] embedding, string title, string description)
        {
            if (embedding == null)
                throw new ArgumentNullException(nameof(embedding));

            if (embedding.Length != _expectedDim)
                throw new ArgumentException(
                    $"Embedding must be {_expectedDim}-dimensional. Actual: {embedding.Length}", nameof(embedding));

            // Prepare payload for Qdrant
            var payload = new
            {
                points = new[]
                {
                    new
                    {
                        id = id, // Already string UUID
                        vector = embedding,
                        payload = new
                        {
                            title = title ?? string.Empty,
                            description = description ?? string.Empty
                        }
                    }
                }
            };

            // Serialize payload to JSON
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Send PUT request to Qdrant
            var response = await _client.PutAsync($"{QdrantUrl}/collections/knowledge_items/points", content);

            // Throw exception if request failed
            if (!response.IsSuccessStatusCode)
            {
                var responseText = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Failed to save point to Qdrant. Status: {response.StatusCode}, Response: {responseText}");
            }
        }
    }
}
