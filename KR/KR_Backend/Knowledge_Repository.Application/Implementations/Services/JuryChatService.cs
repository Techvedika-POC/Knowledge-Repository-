// Knowledge_Repository.Application.Services/JuryChatService.cs
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Application.Dtos.JuryCommunication;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class JuryChatService : IJuryChatService
{
    private readonly IJuryChatRepository _chatRepo;
    private readonly IUserRepository _userRepo;

    public JuryChatService(IJuryChatRepository chatRepo, IUserRepository userRepo)
    {
        _chatRepo = chatRepo;
        _userRepo = userRepo;
    }

    public async Task<Guid> SendMessageAsync(CreateJuryChatMessageDto dto)
    {
        var entity = new JuryChatMessage
        {
            MessageId = Guid.NewGuid(),
            EventId = dto.EventId,
            SenderJuryId = dto.SenderJuryId,
            Message = dto.Message,
            CreatedOn = DateTime.UtcNow,
            ReplyToMessageId = dto.ReplyToMessageId 
        };

        await _chatRepo.AddAsync(entity);
        return entity.MessageId;
    }


    public async Task<List<JuryChatMessageDto>> GetMessagesAsync(Guid eventId, int limit = 100)
    {
        var items = await _chatRepo.GetByEventAsync(eventId, limit);

        return items.Select(m => new JuryChatMessageDto
        {
            MessageId = m.MessageId,
            EventId = m.EventId,
            SenderJuryId = m.SenderJuryId,
            SenderName = m.Sender?.Name ?? string.Empty,
            SenderEmail = m.Sender?.Email ?? string.Empty,
            Message = m.Message,
            CreatedOn = m.CreatedOn,
            ReplyTo = m.ReplyTo == null ? null : new ReplyPreviewDto
            {
                MessageId = m.ReplyTo.MessageId,
                Message = m.ReplyTo.Message,
                SenderName = m.ReplyTo.Sender?.Name ?? "",
                SenderEmail = m.ReplyTo.Sender?.Email ?? "",
                CreatedOn = m.ReplyTo.CreatedOn
            }
        }).ToList();
    }

}
