using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IKnowledgeItemService
    {
        Task<KnowledgeItem> UploadKnowledgeItemAsync(
            KnowledgeItemUploadDto dto,
            Guid userId
        );
    }
}
