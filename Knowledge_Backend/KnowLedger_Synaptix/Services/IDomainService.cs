using KnowLedger_Synaptix.Dtos;

namespace KnowLedger_Synaptix.Services
{
    public interface IDomainService
    {
        Task<List<DomainDto>> GetAllDomainsAsync();
        Task<DomainDto?> GetDomainByIdAsync(Guid domainId);
        Task<DomainDto?> GetDomainByNameAsync(string domainName);
        Task<List<CategoryDto>> GetCategoriesByDomainIdAsync(Guid domainId);
    }
}
