using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos.JuryCommunication
{
    public class FinalScoreDto
    {
        public Guid EventId { get; set; }
        public Guid TeamId { get; set; }
        public Guid? ApprovedBy { get; set; }
        public int TotalScore { get; set; }
        public string? Remarks { get; set; }
    }
}
