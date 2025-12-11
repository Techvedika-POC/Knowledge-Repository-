using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IEventJuryRepository:IGenericRepository<EventJury>
    {
        Task<bool> IsUserEventJuryAsync(Guid userId);
        Task<bool> IsUserJuryForEventAsync(Guid eventId, Guid userId);
        Task<List<Guid>> GetEventIdsForUserAsync(Guid userId);
    }

}
