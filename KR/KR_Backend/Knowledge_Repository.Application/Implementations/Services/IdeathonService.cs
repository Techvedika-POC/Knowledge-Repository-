using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class IdeathonService : IIdeathonService
    {
        private readonly IIdeathonRepository _repo;
        private readonly IEmailService _email;
        private readonly IMentorRepository _mentorRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IEventRepository _eventRepository;

        public IdeathonService(
            IIdeathonRepository repo,
            IEmailService email,
            IMentorRepository mentor,
            IUserRepository user,
            ITeamRepository team,
            IEventRepository eventRepo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _mentorRepository = mentor ?? throw new ArgumentNullException(nameof(mentor));
            _user_repository_guard(user);
            _team_repository_guard(team);
            _event_repository_guard(eventRepo);

            _userRepository = user;
            _teamRepository = team;
            _eventRepository = eventRepo;
        }

        private void _user_repository_guard(IUserRepository user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
        }

        private void _team_repository_guard(ITeamRepository team)
        {
            if (team == null) throw new ArgumentNullException(nameof(team));
        }

        private void _event_repository_guard(IEventRepository eventRepo)
        {
            if (eventRepo == null) throw new ArgumentNullException(nameof(eventRepo));
        }

        public async Task<IEnumerable<User>> GetAvailableMentorsAsync(Guid eventId)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("Invalid event ID.", nameof(eventId));

            return await _repo.GetAvailableMentorsAsync(eventId);
        }
        public async Task AssignMentorToTeamAsync(Guid eventId, Guid teamId, Guid incomingId)
        {
            if (eventId == Guid.Empty || teamId == Guid.Empty || incomingId == Guid.Empty)
                throw new ArgumentException("Invalid IDs");

            await _repo.AssignMentorToTeamAsync(eventId, teamId, incomingId);

     
            var eventInfo = await _eventRepository.GetByIdAsync(eventId)
                ?? throw new InvalidOperationException("Event not found.");

            var team = await _teamRepository.GetByIdAsync(teamId)
                ?? throw new InvalidOperationException("Team not found.");

       
            Mentor mentorRecord = null;

   
            mentorRecord = await _mentorRepository.GetByIdAsync(incomingId);

            if (mentorRecord == null)
            {
                mentorRecord = await _mentorRepository.GetByUserAndTeamAsync(incomingId, teamId);
            }
            if (mentorRecord == null)
            {
                mentorRecord = await _mentor_repository_get_fallback(incomingId, eventId, teamId);
            }

            if (mentorRecord == null)
            {
               
                var byUser = (await _mentorRepository.GetByUserAsync(incomingId)).FirstOrDefault();
                if (byUser != null) mentorRecord = byUser;
            }

            if (mentorRecord == null)
            {
           
                throw new InvalidOperationException("Could not resolve mentor record after assignment.");
            }

  
            if (mentorRecord.UserId == null || mentorRecord.UserId == Guid.Empty)
                throw new InvalidOperationException("Mentor record missing UserId.");

            var mentorUser = await _userRepository.GetByIdAsync(mentorRecord.UserId.Value)

                ?? throw new InvalidOperationException("Mentor user not found.");

          
            var teamMembers = (await _teamRepository.GetTeamMembersAsync(teamId))?.ToList() ?? new List<User>();

            string mentorBody = $@"
<h2>You are Assigned as Mentor</h2>

<p><strong>Event:</strong> {Escape(eventInfo.Title)}</p>
<p><strong>Team:</strong> {Escape(team.TeamName)}</p>

<p><strong>Team Members:</strong></p>
<ul>
    {string.Join("", teamMembers.Select(x => $"<li>{Escape(x.Name)} ({Escape(x.Email)})</li>"))}
</ul>";

            await _email.SendEmailAsync(
                mentorUser.Email,
                $"Mentor Assignment - Team {team.TeamName}",
                mentorBody
            );
            foreach (var member in teamMembers)
            {
                string memberBody = $@"
<h2>Mentor Assigned To Your Team</h2>

<p><strong>Mentor:</strong> {Escape(mentorUser.Name)}</p>
<p><strong>Team:</strong> {Escape(team.TeamName)}</p>
<p><strong>Event:</strong> {Escape(eventInfo.Title)}</p>";

                await _email.SendEmailAsync(
                    member.Email,
                    $"Mentor Assigned - {Escape(mentorUser.Name)}",
                    memberBody
                );
            }
        }

   
        private async Task<Mentor?> _mentor_repository_get_fallback(Guid incomingUserOrMentorId, Guid eventId, Guid teamId)
        {
      
            var m = await _mentorRepository.GetByUserAndTeamAsync(incomingUserOrMentorId, teamId);
            if (m != null) return m;

          
            m = await _mentorRepository.GetByUserAndEventAsync(incomingUserOrMentorId, eventId);
            if (m != null) return m;


            var list = (await _mentorRepository.GetByUserAsync(incomingUserOrMentorId)).ToList();
            if (list.Count == 1) return list.Single();


            var byAssigned = list.FirstOrDefault(x => x.AssignedTeamId == teamId);
            if (byAssigned != null) return byAssigned;


            var byEvent = list.FirstOrDefault(x => x.EventId == eventId);
            if (byEvent != null) return byEvent;

            return null;
        }

      

        private static string Escape(string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return System.Net.WebUtility.HtmlEncode(input);
        }

        public async Task<IEnumerable<Mentor>> GetMentorsForEventAsync(Guid eventId)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("Invalid event ID.", nameof(eventId));

            return await _repo.GetMentorsForEventAsync(eventId);
        }

        public async Task RemoveMentorFromTeamAsync(Guid mentorId)
        {
            if (mentorId == Guid.Empty)
                throw new ArgumentException("Invalid mentor ID.", nameof(mentorId));

            await _repo.RemoveMentorFromTeamAsync(mentorId);
        }

       
        public async Task<IEnumerable<User>> GetAvailableJuryMembersAsync(Guid eventId)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("Invalid event ID.", nameof(eventId));

            return await _repo.GetAvailableJuryMembersAsync(eventId);
        }

        public async Task AssignJuryMembersToEventAsync(Guid eventId, IEnumerable<Guid> juryUserIds)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("Invalid event ID.", nameof(eventId));

            if (juryUserIds == null)
                throw new ArgumentNullException(nameof(juryUserIds));

            var userList = juryUserIds.ToList();

            if (userList.Count == 0)
                throw new ArgumentException("At least one jury member must be selected.", nameof(juryUserIds));

            await _repo.AssignJuryMembersToEventAsync(eventId, userList);
        }

        public async Task<IEnumerable<User>> GetJuryMembersForEventAsync(Guid eventId)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("Invalid event ID.", nameof(eventId));

            return await _repo.GetJuryMembersForEventAsync(eventId);
        }

        public async Task RemoveJuryMemberFromEventAsync(Guid eventId, Guid userId)
        {
            if (eventId == Guid.Empty || userId == Guid.Empty)
                throw new ArgumentException("Invalid event or jury member ID.");

            await _repo.RemoveJuryMemberFromEventAsync(eventId, userId);
        }

     

        public async Task<IEnumerable<Team>> GetTeamsForEventAsync(Guid eventId)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("Invalid event ID.", nameof(eventId));

            return await _repo.GetTeamsForEventAsync(eventId);
        }

        public async Task<Team?> GetTeamByIdAsync(Guid teamId)
        {
            if (teamId == Guid.Empty)
                throw new ArgumentException("Invalid team ID.", nameof(teamId));

            return await _repo.GetTeamByIdAsync(teamId);
        }

       
        public async Task SchedulePresentationAsync(Guid eventId, Guid teamId, DateTime presentationDate)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("Invalid event ID.", nameof(eventId));

            if (teamId == Guid.Empty)
                throw new ArgumentException("Invalid team ID.", nameof(teamId));

            if (presentationDate == default)
                throw new ArgumentException("Invalid presentation date.", nameof(presentationDate));


            await _repo.SchedulePresentationAsync(eventId, teamId, presentationDate);

        
            var eventInfo = await _eventRepository.GetByIdAsync(eventId);
            var team = await _teamRepository.GetByIdAsync(teamId);
            var teamMembers = await _teamRepository.GetTeamMembersAsync(teamId);
            var juryMembers = await _repo.GetJuryMembersForEventAsync(eventId);

            string formattedDate = presentationDate.ToString("dd MMM yyyy hh:mm tt");


            foreach (var member in teamMembers)
            {
                string memberBody = $@"
<h2>Presentation Scheduled</h2>

<p><strong>Event:</strong> {eventInfo.Title}</p>
<p><strong>Team:</strong> {team.TeamName}</p>
<p><strong>Date & Time:</strong> {formattedDate}</p>

<p>Please be prepared for your team's presentation.</p>";

                await _email.SendEmailAsync(
                    member.Email,
                    $"Presentation Scheduled - Team {team.TeamName}",
                    memberBody
                );
            }


            foreach (var jury in juryMembers)
            {
                string juryBody = $@"
<h2>New Presentation Scheduled</h2>

<p><strong>Event:</strong> {eventInfo.Title}</p>

<p><strong>Team:</strong> {team.TeamName}</p>
<p><strong>Presentation Date:</strong> {formattedDate}</p>

<p>You are assigned as Jury for this event. Please review the team’s presentation.</p>";

                await _email.SendEmailAsync(
                    jury.Email,
                    $"New Team Presentation - {team.TeamName}",
                    juryBody
                );
            }
        }


        public async Task<IEnumerable<Presentation>> GetPresentationsForEventAsync(Guid eventId)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("Invalid event ID.", nameof(eventId));

            return await _repo.GetPresentationsForEventAsync(eventId);
        }

        public async Task RemovePresentationAsync(Guid presentationId)
        {
            if (presentationId == Guid.Empty)
                throw new ArgumentException("Invalid presentation ID.");

            await _repo.RemovePresentationAsync(presentationId);
        }
    }
}
