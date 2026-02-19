using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IIdeaService
    {
        Task SubmitIdeaAsync(SubmitIdeaDto dto);
        Task<IdeaSubmission?> GetByTeamAsync(Guid teamId);
    }


}
