using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{
    public class StartInterviewDto
    {
        public Guid UserId { get; set; }
        public Guid ProblemId { get; set; }
    }

    public class InterviewMessageDto
    {
        public Guid InterviewId { get; set; }
        public string Message { get; set; }
    }

    public class InterviewResultDto
    {
        public Guid InterviewId { get; set; }
        public string AiQuestion { get; set; }
        public float? CommunicationScore { get; set; }
        public string AiFeedback { get; set; }
        public bool IsCompleted { get; set; }
    }

}
