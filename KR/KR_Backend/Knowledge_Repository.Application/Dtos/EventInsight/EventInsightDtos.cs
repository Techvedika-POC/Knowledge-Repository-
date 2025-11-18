using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Knowledge_Repository.Application.Dtos.EventInsight
{
    public class UserEventInsightDto
    {
        public Guid EventId { get; set; }
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public double AverageRating { get; set; }

        public List<SubmissionDto> Submissions { get; set; } = new();
        public List<FeedbackDto> Feedbacks { get; set; } = new();
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


    public class FileAttachmentDto
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }

    public class FeedbackDto
    {
        public Guid FeedbackId { get; set; }
        public string MentorName { get; set; } = string.Empty;
        public string FeedbackText { get; set; } = string.Empty;
        public int ProgressRating { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<FeedbackReplyDto>? Replies { get; set; }
    }

    public class FeedbackReplyDto
    {
        public Guid ReplyId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ReplyText { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
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
