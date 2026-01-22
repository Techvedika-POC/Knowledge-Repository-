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
            CreatedOn = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
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

            SenderName = m.SenderJury?.Name ?? string.Empty,
            SenderEmail = m.SenderJury?.Email ?? string.Empty,

            Message = m.Message,
            CreatedOn = m.CreatedOn,
            ReplyTo = m.ReplyToMessage == null ? null : new ReplyPreviewDto
            {
                MessageId = m.ReplyToMessage.MessageId,
                Message = m.ReplyToMessage.Message,

                SenderName = m.ReplyToMessage.SenderJury?.Name ?? "",
                SenderEmail = m.ReplyToMessage.SenderJury?.Email ?? "",

                CreatedOn = m.ReplyToMessage.CreatedOn
            }
        })
        .ToList();
    }


}
