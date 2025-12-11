using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Knowledge_Repository.Application.Dtos;

namespace Knowledge_Repository.Application.Dtos.EventInsight
{
    public class UserEventInsightDto
    {
        public Guid EventId { get; set; }
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public double AverageRating { get; set; }

        public List<SubmissionDto> Submissions { get; set; } = new();
        public List<TeamMemberDto> TeamMembers { get; set; } = new();
    }

    public class SubmissionDto
    {
        public Guid ItemId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SubmittedBy { get; set; } = string.Empty;
        public Guid? CreatedBy { get; set; }
        public List<string>? Tags { get; set; }
        public List<FileAttachmentDto>? Attachments { get; set; }
        public DateTime? CreatedOn { get; set; }
    }


 

    public class EventInsightSummaryDto
    {
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public int SubmissionCount { get; set; }
        public int FeedbackCount { get; set; }
        public double AverageRating { get; set; }
        public DateTime? LastSubmissionDate { get; set; }
    }

    public class SubmissionRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string>? Tags { get; set; }
        public List<IFormFile>? Files { get; set; }
        public Guid EventId { get; set; }
        public Guid TeamId { get; set; }
        public Guid? CreatedBy { get; set; }
    }

}