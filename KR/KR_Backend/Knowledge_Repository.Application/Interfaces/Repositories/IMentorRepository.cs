using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IMentorRepository
    {
        // ---- Mentor Dashboard ----
        Task<IEnumerable<Team>> GetAssignedTeamsAsync(Guid mentorId);
        Task<Team> GetTeamDetailsAsync(Guid teamId);

        // ---- Feedback Management ----
        Task<IEnumerable<TeamFeedback>> GetTeamFeedbacksAsync(Guid teamId);
        Task<IEnumerable<TeamFeedback>> GetFeedbacksByMentorAsync(Guid mentorId);
        Task<TeamFeedback> GetFeedbackByIdAsync(Guid feedbackId);
        Task AddFeedbackAsync(TeamFeedback feedback);
        Task UpdateFeedbackAsync(TeamFeedback feedback);
        Task DeleteFeedbackAsync(Guid feedbackId);

        // ---- Team Progress / Evaluation ----
        Task<double> GetAverageTeamRatingAsync(Guid teamId);
        Task<IEnumerable<(Team team, double averageRating)>> GetTeamsWithProgressAsync(Guid mentorId);
        Task<Mentor> GetMentorByUserIdAsync(Guid userId);

    }
}
