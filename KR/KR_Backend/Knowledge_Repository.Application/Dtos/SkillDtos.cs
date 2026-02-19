using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{
    public class AddSkillDto
    {
        public string Name { get; set; }
    }

    public class UpdateUserSkillDto
    {
        public Guid UserId { get; set; }
        public string SkillName { get; set; } = null!;
        public double Proficiency { get; set; }
    }


    public class UserSkillResponseDto
    {
        public string SkillName { get; set; }
        public double Proficiency { get; set; }
    }
    public class InferUserSkillDto
    {
        public Guid UserId { get; set; }
        public string SkillName { get; set; }
        public double Delta { get; set; } 
    }

    public class SkillSummaryDto
    {
        public string LearningVelocity { get; set; }
        public string StrengthArea { get; set; }
        public string GrowthArea { get; set; }
    }


}
