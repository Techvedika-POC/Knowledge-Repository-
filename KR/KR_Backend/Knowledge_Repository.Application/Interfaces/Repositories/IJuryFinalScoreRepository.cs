using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IJuryFinalScoreRepository : IGenericRepository<JuryFinalScore>
    {
        Task<JuryFinalScore?> GetByEventTeamAsync(Guid eventId, Guid teamId);
        Task<JuryFinalScore?> GetByEventTeamAndApproverAsync(Guid eventId, Guid teamId, Guid approverId);
        Task<bool> ExistsForEventTeamByJuryAsync(Guid eventId, Guid teamId, Guid approvedBy);
        }
}
