using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingPlanParser.Services.Enrichment.Models
{
    public class LessonContext
    {
        public Guid LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }
}
