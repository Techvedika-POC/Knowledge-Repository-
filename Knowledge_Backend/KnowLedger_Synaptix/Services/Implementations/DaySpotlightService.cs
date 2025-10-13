using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Implementations
{
    public class DaySpotlightService : IDaySpotlightService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public DaySpotlightService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        public async Task<DaySpotlightDto> GetDaySpotlightAsync()
        {
            var spotlight = await _context.SpotlightItems
                .OrderByDescending(s => s.CreatedOn)
                .FirstOrDefaultAsync();

            if (spotlight == null) return null;

            return new DaySpotlightDto
            {
                ResourceTitle = spotlight.ContentText,
                ResourceLink = spotlight.ResourceLink,
                Tip = "Use tagging consistently to boost searchability.",
                Quote = "Knowledge grows when shared."
            };
        }
    }
}
