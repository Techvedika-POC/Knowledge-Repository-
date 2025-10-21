using KnowLedger_Synaptix.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IGlobalSearchService
    {
        Task<List<KnowledgeItemFilterDto>> GlobalSearchAsync(string keyword);
    }

}
