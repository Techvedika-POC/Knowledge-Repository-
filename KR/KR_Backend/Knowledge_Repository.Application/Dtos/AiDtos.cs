using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{
    public class GenerateAiInsightRequestDto
    {
        public string EntityType { get; set; }
        public string InsightType { get; set; }  
        public Guid EntityId { get; set; }
        public string Context { get; set; }

        public Guid? UserId { get; set; }
        public Guid? EventId { get; set; }
        public Guid? TeamId { get; set; }
    }


    public class AiInsightResponseDto
    {
        public Guid AiInsightId { get; set; }
        public string EntityType { get; set; }
        public Guid? EntityId { get; set; }
        public string InsightType { get; set; }
        public string OutputResult { get; set; }

        public float? Score { get; set; }   

        public DateTime? CreatedOn { get; set; }
    }

    public class AiChatRequestDto
    {
        public Guid UserId { get; set; }
        public Guid? EventId { get; set; }
        public Guid? TeamId { get; set; }
        public string Message { get; set; }
    }

    public class AiChatMessageDto
    {
        public string Role { get; set; }  
        public string Message { get; set; }
        public DateTime? CreatedOn { get; set; } 
    }


}
