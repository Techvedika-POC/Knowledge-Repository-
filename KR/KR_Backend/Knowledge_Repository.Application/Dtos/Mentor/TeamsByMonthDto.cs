using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos.Mentor
{
    public class TeamsByMonthDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthLabel { get; set; } = string.Empty;
        public List<TeamDetailsDto> Teams { get; set; } = new List<TeamDetailsDto>();
    }
}
