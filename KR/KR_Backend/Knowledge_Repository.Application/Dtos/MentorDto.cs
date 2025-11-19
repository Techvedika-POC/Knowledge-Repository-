using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{
    public class MentorDto
    {
        public Guid MentorId { get; set; }

        public Guid? EventId { get; set; }

        public Guid? UserId { get; set; }

        public Guid? AssignedTeamId { get; set; }

        public DateTime? CreatedOn { get; set; }

        public virtual Team AssignedTeam { get; set; }

        public virtual Event Event { get; set; }

        public virtual ICollection<TeamFeedback> TeamFeedbacks { get; set; } = new List<TeamFeedback>();

        public virtual User User { get; set; }
    }
}
