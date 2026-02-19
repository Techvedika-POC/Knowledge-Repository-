using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{

    public class SubmitJuryScoreDto
    {
        public Guid EventId { get; set; }
        public Guid TeamId { get; set; }
        public Guid JuryId { get; set; }
        public List<JuryScoreInputDto> Scores { get; set; }
    }

    public class JuryScoreInputDto
    {
        public string CriteriaName { get; set; }
        public int Score { get; set; }
        public string? Remarks { get; set; }
    }

    public class RadarChartDto
    {
        public string CriteriaName { get; set; }
        public float Score { get; set; }
    }
    public class TeamRankingDto
    {
        public Guid TeamId { get; set; }
        public string TeamName { get; set; }
        public string EventTitle { get; set; }
        public int TotalScore { get; set; }
        public int MemberCount { get; set; }
    }

    public class HackathonJuryTeamDto
    {
        public Guid TeamId { get; set; }
        public string TeamName { get; set; }

        public string IdeaText { get; set; }
        public string RepoUrl { get; set; }
        public string DemoUrl { get; set; }

        public float? AiScore { get; set; }
        public string AiFeedback { get; set; }
    }


}
