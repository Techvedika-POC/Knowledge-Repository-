using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using System;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IKnowledgeItemService
    {
        Task<KnowledgeItem> UploadKnowledgeItemAsync(KnowledgeItemUploadDto dto, Guid userId);

        Task<KnowledgeItemDetailsDto> GetKnowledgeItemDetailsAsync(Guid itemId);

    }
}
