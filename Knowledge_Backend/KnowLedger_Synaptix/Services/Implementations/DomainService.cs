using KnowLedger_Synaptix.Dtos;
using Microsoft.EntityFrameworkCore;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;

namespace KnowLedger_Synaptix.Services.Implementations
{
    public class DomainService : IDomainService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public DomainService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        public async Task<List<DomainDto>> GetAllDomainsAsync()
        {
            return await _context.Domains
                .Include(d => d.Categories)
                .Select(d => new DomainDto
                {
                    DomainId = d.DomainId,
                    DomainName = d.DomainName,
                    Description = d.Description,
                    CreatedOn = d.CreatedOn,
                    UpdatedOn = d.UpdatedOn,
                    Categories = d.Categories.Select(c => new CategoryDto
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.CategoryName,
                        Description = c.Description,
                        CreatedOn = c.CreatedOn,
                        UpdatedOn = c.UpdatedOn
                    }).ToList()
                })
                .ToListAsync();
        }
        public async Task<DomainDto?> GetDomainByNameAsync(string domainName)
        {
            return await _context.Domains
                .Where(d => d.DomainName.ToLower() == domainName.ToLower()) // case-insensitive
                .Include(d => d.Categories)
                .Select(d => new DomainDto
                {
                    DomainId = d.DomainId,
                    DomainName = d.DomainName,
                    Description = d.Description,
                    CreatedOn = d.CreatedOn,
                    UpdatedOn = d.UpdatedOn,
                    Categories = d.Categories.Select(c => new CategoryDto
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.CategoryName,
                        Description = c.Description,
                        CreatedOn = c.CreatedOn,
                        UpdatedOn = c.UpdatedOn
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }


        public async Task<DomainDto?> GetDomainByIdAsync(Guid domainId)
        {
            return await _context.Domains
                .Where(d => d.DomainId == domainId)
                .Include(d => d.Categories)
                .Select(d => new DomainDto
                {
                    DomainId = d.DomainId,
                    DomainName = d.DomainName,
                    Description = d.Description,
                    CreatedOn = d.CreatedOn,
                    UpdatedOn = d.UpdatedOn,
                    Categories = d.Categories.Select(c => new CategoryDto
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.CategoryName,
                        Description = c.Description,
                        CreatedOn = c.CreatedOn,
                        UpdatedOn = c.UpdatedOn
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }
        public async Task<List<CategoryDto>> GetCategoriesByDomainIdAsync(Guid domainId)
        {
            var domain = await _context.Domains
                .Where(d => d.DomainId == domainId)
                .Include(d => d.Categories)
                .FirstOrDefaultAsync();

            if (domain == null)
                return new List<CategoryDto>();

            return domain.Categories.Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Description = c.Description,
                CreatedOn = c.CreatedOn,
                UpdatedOn = c.UpdatedOn
            }).ToList();
        }

    }
}
