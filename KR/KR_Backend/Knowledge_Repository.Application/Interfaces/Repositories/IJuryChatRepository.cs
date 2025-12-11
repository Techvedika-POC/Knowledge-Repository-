using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IJuryChatRepository : IGenericRepository<JuryChatMessage>
    {
        Task<IEnumerable<JuryChatMessage>> GetByEventAsync(Guid eventId, int limit = 100);
    }
}
