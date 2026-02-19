
using Knowledge_Repository.Application.Dtos.JuryCommunication;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IJuryChatService
    {
        Task<Guid> SendMessageAsync(CreateJuryChatMessageDto dto);
        Task<List<JuryChatMessageDto>> GetMessagesAsync(Guid eventId, int limit = 100);
    }
}
