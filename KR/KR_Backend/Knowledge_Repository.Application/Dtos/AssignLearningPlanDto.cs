using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{
    public class AssignLearningPlanDto
    {
        public Guid PlanId { get; set; }
        public List<Guid> UserIds { get; set; }
    }

}
