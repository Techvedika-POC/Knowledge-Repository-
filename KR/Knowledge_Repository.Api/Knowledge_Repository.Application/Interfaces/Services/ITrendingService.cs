using KnowLedger_Synaptix.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using Knowledge_Repository.Application.Dtos;
namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface ITrendingService
    {
        Task<List<KnowledgeItemDto>> GetTrendingAsync(int top = 5);
    }
}
