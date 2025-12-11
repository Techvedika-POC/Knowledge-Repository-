// Application/Interfaces/Services/IFreeAiClient.cs
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IFreeAiClient
    {
        /// <summary>
        /// Generates structured JSON for a WeekDto based on the input prompt.
        /// </summary>
        /// <param name="prompt">Week plan input text</param>
        /// <returns>JSON string matching WeekDto</returns>
        Task<string> GenerateWeekJsonAsync(string prompt);
    }
}
