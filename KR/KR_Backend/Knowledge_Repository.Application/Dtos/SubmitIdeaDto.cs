using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{
    public class SubmitIdeaDto
    {
        public Guid EventId { get; set; }
        public Guid TeamId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string RepoUrl { get; set; }
        public string DemoUrl { get; set; }
    }

}
