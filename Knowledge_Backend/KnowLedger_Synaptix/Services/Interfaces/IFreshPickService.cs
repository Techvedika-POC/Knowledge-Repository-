using KnowLedger_Synaptix.Dtos;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IFreshPickService
    {
        /// <summary>
        /// Retrieves the  recently upload knowledge articles
        /// </summary>
      
        Task<List<KnowledgeItemFilterDto>> GetFreshPicksAsync(int count = 10);
    }
}
