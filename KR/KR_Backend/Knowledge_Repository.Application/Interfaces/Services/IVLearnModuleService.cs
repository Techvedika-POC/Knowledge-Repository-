using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Domain.Entities;
using KnowLedger_Synaptix.Dtos;
namespace  Knowledge_Repository.Application.Interfaces.Services
{
    public interface IVLearnModuleService
    {
        Task<IEnumerable<VLearnModuleDto>> GetModulesByTopicAndUserAsync(Guid topicId, Guid userId);
        Task<bool> UpdateTestStatusAsync(VLearnTestResultDto result);
        Task<VLearnModuleDto> AddModuleAsync(Guid topicId, CreateModuleDto dto, Guid createdBy);
        Task<IEnumerable<VLearnModuleDto>> GetModulesByTopicAsync(Guid topicId);
    }
}