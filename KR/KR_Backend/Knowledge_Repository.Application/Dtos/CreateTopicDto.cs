using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{
    public class CreateTopicDto
    {
        public string TopicName { get; init; } = string.Empty;
        public string? Description { get; init; }

    }
}
