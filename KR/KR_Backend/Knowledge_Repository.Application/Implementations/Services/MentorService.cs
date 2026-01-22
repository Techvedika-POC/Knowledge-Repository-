using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Dtos.EventInsight;
using Knowledge_Repository.Application.Dtos.Mentor;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class MentorService : IMentorService
    {
        private readonly IMentorRepository _mentorRepo;

        public MentorService(IMentorRepository mentorRepo)
        {
            _mentorRepo = mentorRepo;
        }
        public async Task<IEnumerable<TeamsByMonthDto>> GetTeamsForMentorAsync(Guid mentorId)
        {
            if (mentorId == Guid.Empty)
                throw new ArgumentException("Invalid mentor ID.", nameof(mentorId));

            
            var teams = (await _mentorRepo.GetAssignedTeamsAsync(mentorId))?.Where(t => t != null).ToList()
                        ?? new List<Team>();

            var detailedList = new List<(TeamDetailsDto Details, DateOnly? GroupDate)>();

            foreach (var t in teams)
            {
                TeamDetailsDto details;
                try
                {
                    details = await GetTeamDetailsAsync(t.TeamId);
                }
                catch (KeyNotFoundException)
                {
                   
                    continue;
                }

               
                DateOnly? groupDate = null;
                if (t.Event?.StartDate.HasValue == true)
                {
                    groupDate = t.Event.StartDate.Value;
                }
                else if (t.CreatedOn.HasValue)
                {
                  
                    groupDate = DateOnly.FromDateTime(t.CreatedOn.Value);
                }

                detailedList.Add((details, groupDate));
            }

       
            var grouped = detailedList
                .GroupBy(d =>
                {
                    var gd = d.GroupDate;
                    if (gd.HasValue) return (Year: gd.Value.Year, Month: gd.Value.Month);
                    return (Year: 1, Month: 1); 
                })
                .Select(g =>
                {
                    var year = g.Key.Year;
                    var month = g.Key.Month;
                    var monthLabel = (year == 1 && month == 1)
                        ? "Undated"
                        : new DateTime(year, month, 1).ToString("MMMM yyyy");

                    return new TeamsByMonthDto
                    {
                        Year = year,
                        Month = month,
                        MonthLabel = monthLabel,
                        Teams = g.Select(x => x.Details).OrderBy(d => d.TeamName).ToList()
                    };
                })
               
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .ToList();

           
            var datedGroups = grouped.Where(g => g.Year != 1).ToList();
            var undatedGroups = grouped.Where(g => g.Year == 1).ToList();
            var final = datedGroups.Concat(undatedGroups).ToList();

            return final;
        }



        public async Task<TeamDetailsDto> GetTeamDetailsAsync(Guid teamId)
        {
            if (teamId == Guid.Empty)
                throw new ArgumentException("Invalid team ID.", nameof(teamId));

            var team = await _mentorRepo.GetTeamDetailsAsync(teamId);
            if (team == null)
                throw new KeyNotFoundException("Team not found.");

       

            var members = team.TeamMembers?.Select(m => new MemberDto
            {
                UserId = m.UserId,
                Name = m.User?.Name,
                Email = m.User?.Email,
                Role = m.User?.UserRoleUsers?.FirstOrDefault()?.Role?.RoleName
            }).ToList() ?? new List<MemberDto>();


            var submissions = team.EventKnowledgeItems
                .Where(eki => eki.Item != null)
                .Select(eki => eki.Item)
                .Select(k => new KnowledgeItemDto
                {
                    ItemId = k.ItemId,
                    Title = k.Title,
                    Description = k.Description,

                    Tags = k.KnowledgeTags
                            .Select(t => t.TagName)
                            .ToList(),

                    DomainId = k.DomainId,
                    DomainName = k.Domain?.DomainName,
                    CategoryId = k.CategoryId,
                    CategoryName = k.Category?.CategoryName,

                    OwnerId = k.OwnerId,
                    OwnerName = k.Owner?.Name,

                    Views = k.Engagements.Count(e => e.EngagementType == "View"),
                    Likes = k.Engagements.Count(e => e.EngagementType == "Like"),
                    Comments = k.Engagements.Count(e => e.EngagementType == "Comment"),
                    EngagementScore = k.Engagements.Count(),

                    IsEventItem = k.IsEventItem,
                    CreatedBy = k.CreatedBy,
                    CreatedByName = k.CreatedByNavigation?.Name,
                    UpdatedBy = k.UpdatedBy,
                    UpdatedByName = k.UpdatedByNavigation?.Name,

                    CreatedOn = k.CreatedOn.HasValue
                        ? new DateTimeOffset(k.CreatedOn.Value)
                        : default,

                    UpdatedOn = k.UpdatedOn,

                    Language = k.Language,
                    Framework = k.Framework,
                    Metadata = k.Metadata,

                    KnowledgeItem = k.KnowledgeText,
                    SubmittedBy = k.Owner?.Name
                })
                .ToList();

            return new TeamDetailsDto
            {
                TeamId = team.TeamId,
                TeamName = team.TeamName,
                EventId = team.EventId ?? Guid.Empty,
                Description = team.Event?.Description,
                ProjectTitle = null,
                Members = members,
               
                Submissions = submissions
            };
        }

    }

}
