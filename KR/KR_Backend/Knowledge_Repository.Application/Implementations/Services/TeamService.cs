using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Domain.Entities;

public class TeamService : ITeamService
{
    private readonly ITeamRepository _teamRepo;
    private readonly ITeamMemberRepository _memberRepo;
    private readonly IUserRepository _userRepo;

    private readonly ILearningEventService _learningEventService;
    public TeamService(
        ITeamRepository teamRepo,
        ITeamMemberRepository memberRepo,
        IUserRepository userRepo,
        ILearningEventService learningEventService)
    {
        _teamRepo = teamRepo;
        _memberRepo = memberRepo;
        _userRepo = userRepo;
        _learningEventService = learningEventService;
    }
    public async Task<TeamDto?> GetMyTeamForEvent(Guid eventId, Guid userId)
    {
        var team = await _teamRepo.GetTeamByEventAndUserAsync(eventId, userId);
        if (team == null) return null;

        var members = await _memberRepo.GetMembersByTeamIdAsync(team.TeamId);

        return new TeamDto
        {
            TeamId = team.TeamId,
            TeamName = team.TeamName,
            CreatedBy = team.CreatedBy ?? Guid.Empty,
            CreatedOn = team.CreatedOn ?? DateTime.UtcNow,
            Members = members.Select(m => new TeamMemberDto
            {
                UserId = m.UserId,
                Name = m.User.Name,
                Email = m.User.Email,
                Role = "Member"
            }).ToList()
        };
    }
    public async Task<List<TeamDto>> GetTeamsForEvent(Guid eventId)
    {
        var teams = await _teamRepo.GetByEventIdAsync(eventId);
        var result = new List<TeamDto>();

        foreach (var team in teams)
        {
            var members = await _memberRepo.GetMembersByTeamIdAsync(team.TeamId);

            result.Add(new TeamDto
            {
                TeamId = team.TeamId,
                TeamName = team.TeamName,
                CreatedBy = team.CreatedBy ?? Guid.Empty,
                CreatedOn = team.CreatedOn ?? DateTime.UtcNow,
                Members = members.Select(m => new TeamMemberDto
                {
                    UserId = m.UserId,
                    Name = m.User.Name,
                    Email = m.User.Email,
                    Role = "Member"
                }).ToList()
            });
        }

        return result;
    }
    public async Task AddMemberAsync(Guid teamId, Guid creatorId, string email)
    {
        var team = await _teamRepo.GetByIdAsync(teamId);
        if (team == null)
            throw new Exception("Team not found");

        if (team.CreatedBy != creatorId)
            throw new Exception("Only team creator can add members");

        var user = await _userRepo.GetByEmailAsync(email);
        if (user == null)
            throw new Exception("User not found");

        var exists = await _memberRepo.IsUserInTeamAsync(teamId, user.UserId);
        if (exists)
            throw new Exception("User already in team");

        await _memberRepo.AddAsync(new TeamMember
        {
            TeamMemberId = Guid.NewGuid(),
            TeamId = teamId,
            UserId = user.UserId,
            JoinedOn = DateTime.UtcNow
        });
        await _learningEventService.LogAndProcessAsync(
    creatorId,
    "TEAM_MEMBER_ADDED",
    "Team",
    teamId,
    $"Added member: {email}"
);

    }
    public async Task RemoveMemberAsync(Guid teamId, Guid creatorId, Guid memberId)
    {
        var team = await _teamRepo.GetByIdAsync(teamId);
        if (team == null)
            throw new Exception("Team not found");

        if (team.CreatedBy != creatorId)
            throw new Exception("Only creator can remove members");

        if (memberId == creatorId)
            throw new Exception("Creator cannot be removed");

        var members = await _memberRepo.GetMembersByTeamIdAsync(teamId);
        var member = members.FirstOrDefault(m => m.UserId == memberId);

        if (member != null)
            await _memberRepo.DeleteAsync(member);
    }
}
