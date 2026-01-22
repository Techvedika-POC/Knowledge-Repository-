using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{
    public class TrainingWeekDto
    {
        public int WeekNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public List<string> Prerequisites { get; set; }
        public List<string> LearningObjectives { get; set; }

        public string Metadata { get; set; }

        public List<TrainingModuleDto> Modules { get; set; }
        public List<TrainingAssignmentDto> Assignments { get; set; }
    }

}
