using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;

namespace KnowLedger_Synaptix.Services.Implementations
{
    public class VLearnTopicService : IVLearnTopicService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public VLearnTopicService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Topic>> GetAllTopicsAsync()
        {
            return await _context.Topics
                .AsNoTracking()
                .OrderBy(t => t.TopicName)
                .ToListAsync();
        }
    }
}
