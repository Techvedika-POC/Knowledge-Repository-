using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class AttachmentRepository : GenericRepository<Attachment>, IAttachmentRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public AttachmentRepository(Knowledge_Repository_dbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Attachment>> GetByItemIdAsync(Guid itemId)
        {
            return await _context.Attachments
                .Where(a => a.ItemId == itemId)
                .ToListAsync();
        }
        public async Task AddRangeAsync(IEnumerable<Attachment> attachments)
        {
            await _context.Attachments.AddRangeAsync(attachments);
            await _context.SaveChangesAsync();
        }
    }
}
