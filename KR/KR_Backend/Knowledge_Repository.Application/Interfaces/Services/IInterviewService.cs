using Knowledge_Repository.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IInterviewService
    {
        Task<Guid> StartInterviewAsync(StartInterviewDto dto);
        Task<InterviewResultDto> SendMessageAsync(InterviewMessageDto dto);
    }

}
