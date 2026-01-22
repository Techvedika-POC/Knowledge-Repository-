using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{
    public class TrainingAssignmentDto
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public string ExpectedOutput { get; set; }
        public string RubricJson { get; set; }

        public string AssignmentType { get; set; }
        public int EstimatedDurationMinutes { get; set; }

        public string Metadata { get; set; }
    }

}
