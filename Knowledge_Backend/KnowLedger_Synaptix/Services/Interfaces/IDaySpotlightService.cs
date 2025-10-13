using KnowLedger_Synaptix.Dtos;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IDaySpotlightService
    {
        Task<DaySpotlightDto> GetDaySpotlightAsync();
    }
}
