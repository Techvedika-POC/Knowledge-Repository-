using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    /// <summary>
    /// Provides high-level operations for managing domains and their related categories.
    /// </summary>
    public class DomainService : IDomainService
    {
        private readonly IDomainRepository _domainRepository;

        public DomainService(IDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task<List<Domains>> GetAllDomainsAsync()
        {
            var domains = await _domainRepository.GetAllAsync();

            return domains
                .Select(d => new Domains
                {
                    DomainId = d.DomainId,
                    DomainName = d.DomainName,
                    Description = d.Description,
                    CreatedOn = d.CreatedOn,
                    UpdatedOn = d.UpdatedOn,
                    Categories = d.Categories?.Select(c => new Category
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.CategoryName,
                        Description = c.Description,
                        CreatedOn = c.CreatedOn,
                        UpdatedOn = c.UpdatedOn
                    }).ToList()
                })
                .ToList();
        }

        public async Task<Domains?> GetDomainByIdAsync(Guid domainId)
        {
            var domain = await _domainRepository.GetByIdAsync(domainId);
            if (domain == null) return null;

            return new Domains
            {
                DomainId = domain.DomainId,
                DomainName = domain.DomainName,
                Description = domain.Description,
                CreatedOn = domain.CreatedOn,
                UpdatedOn = domain.UpdatedOn,
                Categories = domain.Categories?.Select(c => new Category
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    CreatedOn = c.CreatedOn,
                    UpdatedOn = c.UpdatedOn
                }).ToList()
            };
        }

        public async Task<Domains?> GetDomainByNameAsync(string domainName)
        {
            var domain = await _domainRepository.GetByNameAsync(domainName);
            if (domain == null) return null;

            return new Domains
            {
                DomainId = domain.DomainId,
                DomainName = domain.DomainName,
                Description = domain.Description,
                CreatedOn = domain.CreatedOn,
                UpdatedOn = domain.UpdatedOn,
                Categories = domain.Categories?.Select(c => new Category
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    CreatedOn = c.CreatedOn,
                    UpdatedOn = c.UpdatedOn
                }).ToList()
            };
        }

        public async Task<List<Category>> GetCategoriesByDomainIdAsync(Guid domainId)
        {
            var categories = await _domainRepository.GetCategoriesByDomainIdAsync(domainId);
            return categories.Select(c => new Category
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Description = c.Description,
                CreatedOn = c.CreatedOn,
                UpdatedOn = c.UpdatedOn
            }).ToList();
        }
        public async Task AddDomainAsync(Domains domain)
        {
            domain.DomainId = Guid.NewGuid();
            domain.CreatedOn = DateTime.UtcNow;
            await _domainRepository.AddAsync(domain);
        }

        public async Task<bool> UpdateDomainAsync(Guid id, Domains updatedDomain)
        {
            var existing = await _domainRepository.GetByIdAsync(id);
            if (existing == null) return false;

            existing.DomainName = updatedDomain.DomainName;
            existing.Description = updatedDomain.Description;
            existing.UpdatedOn = DateTime.UtcNow;

            await _domainRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteDomainAsync(Guid id)
        {
            var existing = await _domainRepository.GetByIdAsync(id);
            if (existing == null) return false;

            await _domainRepository.DeleteAsync(existing);
            return true;
        }

    }
}
