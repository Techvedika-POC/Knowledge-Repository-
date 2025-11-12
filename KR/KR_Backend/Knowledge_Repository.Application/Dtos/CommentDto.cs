using System;
using System.Collections.Generic;

namespace Knowledge_Repository.Application.Dtos
{
    public class CommentDto
    {
        public Guid? EngagementId { get; set; }
        public Guid ItemId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string CommentText { get; set; } = string.Empty;
        public Guid? ParentCommentId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public List<CommentDto> Replies { get; set; } = new();
    }
}
