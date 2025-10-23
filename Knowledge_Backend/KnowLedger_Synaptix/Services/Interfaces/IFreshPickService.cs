using KnowLedger_Synaptix.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IFreshPickService
    {
        Task<List<KnowledgeItemDto>> GetFreshPicksAsync(int count = 10);
    }
}
