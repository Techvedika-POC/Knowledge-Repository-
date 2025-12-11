using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Domain.Entities
{
    public partial class JuryFinalScore
    {
        public Guid FinalId { get; set; }

        public Guid EventId { get; set; }

        public Guid TeamId { get; set; }

        public Guid? ApprovedBy { get; set; }

        public int? TotalScore { get; set; }

        public string Remarks { get; set; }

        public DateTime? ApprovedOn { get; set; }

        public virtual User ApprovedByNavigation { get; set; }

        public virtual Event Event { get; set; }

        public virtual Team Team { get; set; }
    }
}
