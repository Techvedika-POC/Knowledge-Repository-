using Knowledge_Repository.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{
    public class TrainingLessonDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string LessonType { get; set; }

        public int OrderIndex { get; set; }
        public int DurationMinutes { get; set; }

        public string Overview { get; set; }
        public List<string> Prerequisites { get; set; }

        public string Metadata { get; set; }

        // ✅ ADD THESE
        public List<TrainingResourceDto> Resources { get; set; } = new();
        public List<TrainingAssessmentDto> Assessments { get; set; } = new();
    }
}

