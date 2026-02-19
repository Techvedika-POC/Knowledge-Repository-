
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IFreeAiClient
    {
        Task<string> GenerateWeekJsonAsync(string prompt);
    }
}
