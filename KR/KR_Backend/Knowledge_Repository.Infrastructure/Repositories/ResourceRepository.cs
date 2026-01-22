using Application.Interfaces;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class ResourceRepository : GenericRepository<Resource>, IResourceRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public ResourceRepository(Knowledge_Repository_dbContext context)
            : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Resource>> GetByTopicOrModuleAsync(Guid topicId, Guid? moduleId)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(r => r.TopicId == topicId);

            if (moduleId.HasValue)
                query = query.Where(r => r.ModuleId == moduleId.Value);

            return await query
                .OrderBy(r => r.Title)
                .ToListAsync();
        }

        public async Task AddBatchAsync(IEnumerable<Resource> resources)
        {
            if (resources == null || !resources.Any())
                return;

            foreach (var r in resources)
            {
                if (r.CreatedOn.HasValue)
                    r.CreatedOn = DateTime.SpecifyKind(r.CreatedOn.Value, DateTimeKind.Unspecified);

                if (r.UpdatedOn.HasValue)
                    r.UpdatedOn = DateTime.SpecifyKind(r.UpdatedOn.Value, DateTimeKind.Unspecified);
            }

            await _dbSet.AddRangeAsync(resources);
            await _context.SaveChangesAsync();
        }
    }
}
