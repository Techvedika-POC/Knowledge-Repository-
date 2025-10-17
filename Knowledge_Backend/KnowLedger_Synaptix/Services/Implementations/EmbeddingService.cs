using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace KnowLedger_Synaptix.Services.Implementations
{
    public class EmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _client;
        private readonly ILogger<EmbeddingService> _logger;
        private const string EmbeddingEndpoint = "http://localhost:5001/api/embedding";
        private const int MaxChunkSize = 3000;

        public EmbeddingService(HttpClient client, ILogger<EmbeddingService> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client.Timeout = TimeSpan.FromMinutes(5);
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Empty text passed to GetEmbeddingAsync.");
                return Array.Empty<float>();
            }

            try
            {
                var chunks = SplitTextIntoChunks(text, MaxChunkSize);
                var chunkEmbeddings = new List<float[]>();

                foreach (var chunk in chunks)
                {
                    var json = JsonSerializer.Serialize(new { text = chunk });
                    using var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _client.PostAsync(EmbeddingEndpoint, content);
                    var respText = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Embedding API response (first 1000 chars): {Resp}",
                        respText.Length > 1000 ? respText.Substring(0, 1000) : respText);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError("Embedding request failed with status code {StatusCode}, body: {Body}",
                            response.StatusCode, respText);
                        continue;
                    }

                    float[]? emb = null;

                    try
                    {
                        using var doc = JsonDocument.Parse(respText);
                        var root = doc.RootElement;

                        if (root.TryGetProperty("embedding", out var eProp) && eProp.ValueKind == JsonValueKind.Array)
                        {
                            emb = eProp.EnumerateArray().Select(x => x.GetSingle()).ToArray();
                        }
                        else if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Array)
                        {
                            var first = dataProp.EnumerateArray().FirstOrDefault();
                            if (first.ValueKind == JsonValueKind.Object && first.TryGetProperty("embedding", out var e2))
                                emb = e2.EnumerateArray().Select(x => x.GetSingle()).ToArray();
                        }
                        else if (root.TryGetProperty("result", out var rProp) && rProp.TryGetProperty("embedding", out var e3))
                        {
                            emb = e3.EnumerateArray().Select(x => x.GetSingle()).ToArray();
                        }
                    }
                    catch (Exception parseEx)
                    {
                        _logger.LogError(parseEx, "Failed to parse embedding response JSON: {Resp}", respText);
                    }

                    if (emb == null || emb.Length == 0)
                    {
                        _logger.LogWarning("No embedding found in API response for chunk (length {Len})", chunk.Length);
                        continue;
                    }

                    chunkEmbeddings.Add(emb);
                }

                if (chunkEmbeddings.Count == 0)
                {
                    _logger.LogWarning("No chunk embeddings returned for text of length {Len}", text.Length);
                    return Array.Empty<float>();
                }

                return AverageEmbeddings(chunkEmbeddings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while generating embedding.");
                return Array.Empty<float>();
            }
        }

        private static List<string> SplitTextIntoChunks(string text, int chunkSize)
        {
            var chunks = new List<string>();
            int offset = 0;
            while (offset < text.Length)
            {
                int size = Math.Min(chunkSize, text.Length - offset);
                chunks.Add(text.Substring(offset, size));
                offset += size;
            }
            return chunks;
        }

        private static float[] AverageEmbeddings(List<float[]> embeddings)
        {
            int dim = embeddings[0].Length;
            var avg = new float[dim];
            foreach (var emb in embeddings)
            {
                if (emb.Length != dim)
                    throw new InvalidOperationException("Chunk embeddings have inconsistent dimension.");
                for (int i = 0; i < dim; i++) avg[i] += emb[i];
            }
            for (int i = 0; i < dim; i++) avg[i] /= embeddings.Count;
            return avg;
        }
    }
}
