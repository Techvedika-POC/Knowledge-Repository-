using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IVLearnModuleService
    {
        Task<IEnumerable<VLearnModuleDto>> GetModulesByTopicAndUserAsync(Guid topicId, Guid userId);
        Task<bool> UpdateTestStatusAsync(VLearnTestResultDto result);
    }
}
