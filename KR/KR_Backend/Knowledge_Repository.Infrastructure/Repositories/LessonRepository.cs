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
    public class LessonRepository : GenericRepository<Lesson>, ILessonRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public LessonRepository(Knowledge_Repository_dbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Lesson>> GetByModuleIdAsync(Guid moduleId)
        {
            return await _dbSet
                .Where(l => l.ModuleId == moduleId)
                .OrderBy(l => l.OrderIndex)
                .ToListAsync();
        }

        public async Task AddBatchAsync(IEnumerable<Lesson> lessons)
        {
            await _dbSet.AddRangeAsync(lessons);
            await _context.SaveChangesAsync();
        }
    }
}
