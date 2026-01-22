using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace TrainingPlanParser.Services.Enrichment.Models
{
    public class WeekContext
    {
        public Guid WeekId { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<LessonContext> Lessons { get; set; } = new();
    }
}

