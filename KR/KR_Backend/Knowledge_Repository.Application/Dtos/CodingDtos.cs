using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Knowledge_Repository.Application.Dtos;

public class CreateCodingChallengeDto
{
    public Guid EventId { get; set; }
    public string Title { get; set; }
    public string Difficulty { get; set; }
    public string Description { get; set; }
    public int TimeLimitMinutes { get; set; }
}
public class ChallengeProblemDto
{
    public Guid ProblemId { get; set; }
    public string Title { get; set; }
    public string ProblemStatement { get; set; }
}

public class SubmitCodeDto
{
    public Guid ProblemId { get; set; }
    public Guid UserId { get; set; }
    public string Language { get; set; }
    public string SourceCode { get; set; }
}
public class CodingChallengeMetricsDto
{
    public int ActiveChallenges { get; set; }
    public int Participants { get; set; }
    public double TopScore { get; set; }
    public string Difficulty { get; set; }
}


