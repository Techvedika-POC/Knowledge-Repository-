using KnowLedger_Synaptix.Dtos;
namespace  Knowledge_Repository.Application.Interfaces.Services
{
    public interface IVLearnModuleService
    {
        Task<IEnumerable<VLearnModuleDto>> GetModulesByTopicAndUserAsync(Guid topicId, Guid userId);
        Task<bool> UpdateTestStatusAsync(VLearnTestResultDto result);
    }
}