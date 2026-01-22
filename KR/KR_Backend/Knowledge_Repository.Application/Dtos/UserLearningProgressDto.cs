using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{
    public class UserLearningProgressDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }

        public string PlanStatus { get; set; }
        public decimal ProgressPercent { get; set; }

        public int TotalModules { get; set; }
        public int CompletedModules { get; set; }

        public double? LatestAssessmentScore { get; set; }
        public bool? Passed { get; set; }
    }

}
