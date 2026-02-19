using Knowledge_Repository.Application.Interfaces.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Knowledge_Repository.Infrastructure.Services
{
    public sealed class OpenAiClient : ILlmClient
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public OpenAiClient(HttpClient http)
        {
            _http = http;

            var rawKey = Environment.GetEnvironmentVariable("OPENAI_CODING_API_KEY");

            if (string.IsNullOrWhiteSpace(rawKey))
                throw new InvalidOperationException(
                    "OPENAI_CODING_API_KEY environment variable is missing."
                );
            _apiKey = rawKey
                .Replace("\r", "")
                .Replace("\n", "")
                .Trim();
        }

        public async Task<string> GenerateAsync(string prompt)
        {
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            var body = new
            {
                model = "gpt-3.5-turbo", 
                messages = new[]
                {
                    new { role = "system", content = "You are a senior software engineer and interviewer." },
                    new { role = "user", content = prompt }
                }
            };

            var response = await _http.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json"
                )
            );

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!response.IsSuccessStatusCode)
            {
                var message = root.TryGetProperty("error", out var error)
                    ? error.GetProperty("message").GetString()
                    : "Unknown OpenAI error";

                throw new InvalidOperationException($"OpenAI error: {message}");
            }

            return root
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()!;
        }
    }
}
