using Knowledge_Repository.Application.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IGlobalSearchService
    {
        Task<List<KnowledgeItemDto>> GlobalSearchAsync(string keyword);
    }
}
