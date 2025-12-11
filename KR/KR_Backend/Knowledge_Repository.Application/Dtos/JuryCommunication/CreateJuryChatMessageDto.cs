// File: Knowledge_Repository.Application.Dtos.JuryCommunication/JuryChatDtos.cs
using System;
using System.Collections.Generic;

namespace Knowledge_Repository.Application.Dtos.JuryCommunication
{
    public class CreateJuryChatMessageDto
    {
        public Guid EventId { get; set; }
        public Guid SenderJuryId { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? ReplyToMessageId { get; set; } // <-- new optional field
    }

    public class ReplyPreviewDto
    {
        public Guid MessageId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
    }

    public class JuryChatMessageDto
    {
        public Guid MessageId { get; set; }
        public Guid EventId { get; set; }
        public Guid SenderJuryId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }

        public ReplyPreviewDto ReplyTo { get; set; } = null;
    }


    public class TeamWithMembersDto
    {
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string TeamDescription { get; set; } = string.Empty;
        public List<TeamMemberDto> Members { get; set; } = new List<TeamMemberDto>();
    }

    public class TeamMemberDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
