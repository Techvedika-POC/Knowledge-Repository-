using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{
    public class LeaderboardRowDto
    {
        public int Rank { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }

        public double BestCodeScore { get; set; }
        public double BestInterviewScore { get; set; }
        public double OverallScore { get; set; }
        public int Submissions { get; set; }

        public DateTime? LastActive { get; set; }
    }

    public class LeaderboardRawRow
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }

        public double BestCode { get; set; }
        public int Submissions { get; set; }

        public DateTime? LastCode { get; set; }
        public double BestInterview { get; set; }
        public DateTime? LastInterview { get; set; }
    }






}
