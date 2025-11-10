using Knowledge_Repository.Application.Dtos;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IDaySpotlightService
    {
        Task<DaySpotlightDto?> GetDaySpotlightAsync();
    }
}
