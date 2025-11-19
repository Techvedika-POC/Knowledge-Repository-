using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{
    public class EventsByTypeDto
    {
        public string EventType { get; set; } = string.Empty;
        public List<EventsByMonthDto> Months { get; set; } = new List<EventsByMonthDto>();
    }
}
