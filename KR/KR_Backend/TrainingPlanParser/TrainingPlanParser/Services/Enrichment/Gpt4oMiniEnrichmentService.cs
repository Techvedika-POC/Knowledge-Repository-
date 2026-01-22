using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace TrainingPlanParser.Services.Enrichment
{
    public class Gpt4oMiniEnrichmentService : IEnrichmentLlmService
    {
        private readonly HttpClient _http;

        private const int MaxRetries = 5;
        private const int InitialBackoffMs = 1000;

        public Gpt4oMiniEnrichmentService(string apiKey)
        {
            _http = new HttpClient();
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        public Task<string> GenerateBatchAsync(string prompt)
        {
            return GenerateAsync(prompt);
        }

        public async Task<string> GenerateAsync(string prompt)
        {
            var payload = new
            {
                model = "gpt-4o-mini",
                temperature = 0.3,
                max_tokens = 900,
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content =
@"You generate structured educational enrichment content.

Rules:
- Follow the structure explicitly requested by the user
- Output ONLY the requested content
- Do NOT explain your answer
- Do NOT include markdown or commentary
- Ensure output is complete and non-empty"
                    },
                    new { role = "user", content = prompt }
                }
            };

            var body = JsonConvert.SerializeObject(payload);
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            HttpResponseMessage response;

            try
            {
                response = await PostWithRetryAsync(
                    "https://api.openai.com/v1/chat/completions",
                    content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[LLM ERROR] Retry attempts exhausted");
                Console.WriteLine(ex);
                return string.Empty;
            }

            Console.WriteLine($"[LLM] HTTP Status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("[LLM ERROR] Non-success status");
                return string.Empty;
            }

            try
            {
                var responseText = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(responseText);

                var output =
                    json["choices"]?
                        .FirstOrDefault()?["message"]?["content"]?
                        .ToString();

                if (string.IsNullOrWhiteSpace(output))
                {
                    Console.WriteLine("[LLM] Content is EMPTY or WHITESPACE");
                    return string.Empty;
                }

                return output.Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[LLM PARSE ERROR]");
                Console.WriteLine(ex);
                return string.Empty;
            }
        }

        // =====================================================
        // 🔁 RETRY WITH EXPONENTIAL BACKOFF (429 SAFE)
        // =====================================================
        private async Task<HttpResponseMessage> PostWithRetryAsync(
            string url,
            HttpContent content)
        {
            int delayMs = InitialBackoffMs;

            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                var response = await _http.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                    return response;

                if (response.StatusCode != HttpStatusCode.TooManyRequests)
                    return response;

                Console.WriteLine(
                    $"[LLM] 429 TooManyRequests — retry {attempt}/{MaxRetries} in {delayMs}ms");

                await Task.Delay(delayMs);
                delayMs *= 2; // exponential backoff
            }

            throw new HttpRequestException(
                "LLM rate limit exceeded after retry attempts");
        }
    }
}
