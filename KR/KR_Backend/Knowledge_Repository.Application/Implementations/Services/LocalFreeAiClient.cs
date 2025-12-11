// Infrastructure/Services/LocalFreeAiClient.cs
using Knowledge_Repository.Application.Interfaces.Services;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Services
{
    public class LocalFreeAiClient : IFreeAiClient
    {
        private readonly HttpClient _httpClient;

        public LocalFreeAiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GenerateWeekJsonAsync(string prompt)
        {
            var payload = new { prompt };
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Assume your local Python AI server runs on http://localhost:5000/generate
            var response = await _httpClient.PostAsync("http://localhost:5000/generate", content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
