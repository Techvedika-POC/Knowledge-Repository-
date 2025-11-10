using Knowledge_Repository.Application.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IGlobalSearchService
    {
        /// <summary>
        /// Performs a global keyword-based search across knowledge items, tags, and attachments.
        /// </summary>
        Task<List<KnowledgeItemDto>> GlobalSearchAsync(string keyword);
    }
}
