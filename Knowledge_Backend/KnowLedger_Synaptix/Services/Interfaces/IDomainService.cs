using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    /// <summary>
    /// Provides operations to manage and retrieve domain data and related categories.
    /// </summary>
    public interface IDomainService
    {
        /// <summary>
        /// Retrieves all domains available in the system.
    
        Task<List<Domain>> GetAllDomainsAsync();

        /// <summary>
        /// Retrieves a domain by its unique identifier.
        /// </summary>
      
        Task<Domain?> GetDomainByIdAsync(Guid domainId);

        /// <summary>
        /// Retrieves a domain by its name.
        /// </summary>
     
        Task<Domain?> GetDomainByNameAsync(string domainName);

        /// <summary>
        /// Retrieves all categories associated with a specific domain.
        /// </summary>
   
        Task<List<Category>> GetCategoriesByDomainIdAsync(Guid domainId);
    }
}
