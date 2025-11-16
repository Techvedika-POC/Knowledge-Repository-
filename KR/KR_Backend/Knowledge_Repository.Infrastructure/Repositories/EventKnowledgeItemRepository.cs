using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class EventKnowledgeItemRepository : GenericRepository<EventKnowledgeItem>, IEventKnowledgeItemRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public EventKnowledgeItemRepository(Knowledge_Repository_dbContext context) : base(context)
        {
            _context = context;
        }
        public override async Task<EventKnowledgeItem> AddAsync(EventKnowledgeItem item)
        {
            await _context.EventKnowledgeItems.AddAsync(item);
            await _context.SaveChangesAsync();
            return item;
        }
    }
}