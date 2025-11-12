using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IDomainRepository : IGenericRepository<Domains>
    {
        Task<Domains?> GetByNameAsync(string domainName);
        Task<List<Category>> GetCategoriesByDomainIdAsync(Guid domainId);
    }
}
