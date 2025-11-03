using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Services.Interfaces;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace KnowLedger_Synaptix.Services.Implementations
{
    public class VLearnTestService : IVLearnTestService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<VLearnTestService> _logger;
        private readonly IConfiguration _config;

        public VLearnTestService(HttpClient httpClient, ILogger<VLearnTestService> logger, IConfiguration config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config;
        }

        public async Task<VLearnQuestionResponseDto> GenerateQuestionsAsync(VLearnQuestionRequestDto request)
        {
            var apiUrl = "https://api.opexams.com/questions-generator";
            var apiKey = _config["OpExams:ApiKey"]; // store your key in appsettings.json or user secrets

            var body = new
            {
                type = request.Type,
                topic = request.ModuleName
            };

            var jsonBody = JsonSerializer.Serialize(body);
            var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            try
            {
                var response = await _httpClient.PostAsync(apiUrl, httpContent);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                // Adjust based on OpExams API response structure
                var data = JsonSerializer.Deserialize<VLearnQuestionResponseDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return data ?? new VLearnQuestionResponseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating questions for module {Module}", request.ModuleName);
                return new VLearnQuestionResponseDto();
            }
        }
    }
}
