using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
 
    public interface IDomainService
    {
        Task<List<Domains>> GetAllDomainsAsync();
        Task<Domains?> GetDomainByIdAsync(Guid domainId);
        Task<Domains?> GetDomainByNameAsync(string domainName);
        Task<List<Category>> GetCategoriesByDomainIdAsync(Guid domainId);
        Task AddDomainAsync(Domains domain);
        Task<bool> UpdateDomainAsync(Guid id, Domains updatedDomain);
        Task<bool> DeleteDomainAsync(Guid id);
    }
}
