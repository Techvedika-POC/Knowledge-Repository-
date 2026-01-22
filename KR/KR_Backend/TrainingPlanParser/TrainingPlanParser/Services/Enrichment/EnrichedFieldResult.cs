using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingPlanParser.Services.Enrichment
{
    public class EnrichedFieldResult
    {
        public string FieldName { get; set; } = string.Empty;
        public object GeneratedValue { get; set; } = default!;
        public bool IsCritical { get; set; }
        public string? ContextSummary { get; set; }
    }

}
