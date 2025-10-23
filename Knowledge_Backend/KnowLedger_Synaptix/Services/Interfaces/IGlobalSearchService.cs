using KnowLedger_Synaptix.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IGlobalSearchService
    {
        /// <summary>
        /// Searches for knowledge items matching the specified keyword.
        /// </summary>
        Task<List<KnowledgeItemDto>> GlobalSearchAsync(string keyword);
    }
}
