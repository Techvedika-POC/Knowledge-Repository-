using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class VLearnTopicRepository : GenericRepository<Topic>, IVLearnTopicRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public VLearnTopicRepository(Knowledge_Repository_dbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Topic>> GetAllTopicsAsync()
        {
            return await _context.Topics
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<Topic> AddTopicAsync(Topic topic)
        {
            await AddAsync(topic);
            return topic;
        }

        public async Task<bool> TopicNameExistsAsync(string topicName)
        {
            if (string.IsNullOrWhiteSpace(topicName)) return false;
            return await _context.Topics.AnyAsync(t => t.TopicName.ToLower() == topicName.ToLower());
        }
        public async Task<(IEnumerable<Topic> Items, int Total)> SearchTopicsAsync(string? q, int page, int size)
        {
            var query = _context.Topics.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var ql = q.Trim().ToLower();
                query = query.Where(t => EF.Functions.ILike(t.TopicName, $"%{ql}%")
                                       || EF.Functions.ILike(t.Description, $"%{ql}%"));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.TopicName)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return (items, total);
        }

    }
}
