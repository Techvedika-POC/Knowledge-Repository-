using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{
    public class TrainingResourceDto
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string ResourceType { get; set; }
        public string Description { get; set; }
        public string Metadata { get; set; }
    }
}

