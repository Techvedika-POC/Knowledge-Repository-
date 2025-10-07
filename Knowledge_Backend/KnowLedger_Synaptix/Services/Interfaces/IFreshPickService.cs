using KnowLedger_Synaptix.Dtos;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IFreshPickService
    {
        Task<List<FreshPickDto>> GetFreshPicksAsync(int count = 10);
    }
}
