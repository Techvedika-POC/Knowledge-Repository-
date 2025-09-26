using KnowLedger_Synaptix.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IGlobalSearchService
    {
        /// <summary>
        /// Performs a global search across Knowledge Items and Events.
        /// Includes Domain, Category, Tags, Attachments, and main text fields.
        /// </summary>
        /// <param name="keyword">Search keyword</param>
        /// <returns>List of search results</returns>
        Task<List<GlobalSearchResultDto>> GlobalSearchAsync(string keyword);
    }
}
