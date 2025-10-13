using KnowLedger_Synaptix.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface ITrendingService
    {
        Task<List<TrendingDto>> GetTrendingAsync(int top = 5);
    }
}
