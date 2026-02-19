using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{
    public class TeamDto
    {
        public Guid TeamId { get; set; }
        public string TeamName { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        public List<TeamMemberDto> Members { get; set; } = new();
    }


}
