using Knowledge_Repository.Application.Interfaces.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Knowledge_Repository.Infrastructure.Services
{
    public sealed class GroqClient : ILlmClient
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public GroqClient(HttpClient http)
        {
            _http = http;
            _http.Timeout = TimeSpan.FromSeconds(30); 

            var rawKey = Environment.GetEnvironmentVariable("CODING_CHALLENGE_API_KEY");

            if (string.IsNullOrWhiteSpace(rawKey))
                throw new InvalidOperationException(
                    "CODING_CHALLENGE_API_KEY environment variable is missing."
                );

            _apiKey = rawKey.Trim();
        }

        public async Task<string> GenerateAsync(string prompt)
        {
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            var body = new
            {
                model = "llama-3.3-70b-versatile", 
                messages = new[]
                {
                    new { role = "system", content = "You are a senior software engineer and interviewer." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 1024
            };

            var response = await _http.PostAsync(
                "https://api.groq.com/openai/v1/chat/completions",
                new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json"
                )
            );

            var json = await response.Content.ReadAsStringAsync();

            if ((int)response.StatusCode == 429)
            {
                await Task.Delay(1000);
                return await GenerateAsync(prompt);
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!response.IsSuccessStatusCode)
            {
                var message = root.TryGetProperty("error", out var error)
                    ? error.GetProperty("message").GetString()
                    : json;

                throw new InvalidOperationException($"Groq error: {message}");
            }

            return root
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()!;
        }
    }
}
