using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class JuryFinalScoreRepository : GenericRepository<JuryFinalScore>, IJuryFinalScoreRepository
{
    private readonly Knowledge_Repository_dbContext _context;

    public JuryFinalScoreRepository(Knowledge_Repository_dbContext context)
        : base(context) 
    {
        _context = context;
    }

    public async Task<JuryFinalScore?> GetByEventTeamAsync(Guid eventId, Guid teamId)
    {
        return await _context.JuryFinalScores
            .FirstOrDefaultAsync(x => x.EventId == eventId && x.TeamId == teamId);
    }
    public async Task<JuryFinalScore?> GetByEventTeamAndApproverAsync(Guid eventId, Guid teamId, Guid approverId)
    {
        return await _context.JuryFinalScores
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.EventId == eventId &&
                s.TeamId == teamId &&
                s.ApprovedBy.HasValue &&
                s.ApprovedBy.Value == approverId);
    }
    public async Task<bool> ExistsForEventTeamByJuryAsync(Guid eventId, Guid teamId, Guid approvedBy)
    {
        return await _context.JuryFinalScores
            .AnyAsync(x => x.EventId == eventId && x.TeamId == teamId && x.ApprovedBy == approvedBy);
    }


}

