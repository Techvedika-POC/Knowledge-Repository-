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
        //Get All Domains available
        public async Task<List<Domain>> GetAllDomainsAsync()
        {
            return await _context.Domains
                .Include(d => d.Categories)
                .Select(d => new Domain
                {
                    DomainId = d.DomainId,
                    DomainName = d.DomainName,
                    Description = d.Description,
                    CreatedOn = d.CreatedOn,
                    UpdatedOn = d.UpdatedOn,
                    Categories = d.Categories.Select(c => new Category
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
        //Get Domains by name
        public async Task<Domain?> GetDomainByNameAsync(string domainName)
        {
            return await _context.Domains
                .Where(d => d.DomainName.ToLower() == domainName.ToLower()) 
                .Include(d => d.Categories)
                .Select(d => new Domain
                {
                    DomainId = d.DomainId,
                    DomainName = d.DomainName,
                    Description = d.Description,
                    CreatedOn = d.CreatedOn,
                    UpdatedOn = d.UpdatedOn,
                    Categories = d.Categories.Select(c => new Category
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

        //Get domains by id
        public async Task<Domain?> GetDomainByIdAsync(Guid domainId)
        {
            return await _context.Domains
                .Where(d => d.DomainId == domainId)
                .Include(d => d.Categories)
                .Select(d => new Domain
                {
                    DomainId = d.DomainId,
                    DomainName = d.DomainName,
                    Description = d.Description,
                    CreatedOn = d.CreatedOn,
                    UpdatedOn = d.UpdatedOn,
                    Categories = d.Categories.Select(c => new Category
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
        //get all categories available related to particular domain
        public async Task<List<Category>> GetCategoriesByDomainIdAsync(Guid domainId)
        {
            var domain = await _context.Domains
                .Where(d => d.DomainId == domainId)
                .Include(d => d.Categories)
                .FirstOrDefaultAsync();

            if (domain == null)
                return new List<Category>();

            return domain.Categories.Select(c => new Category
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
