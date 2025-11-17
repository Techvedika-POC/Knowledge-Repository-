// Application/Interfaces/Repositories/IAttachmentRepository.cs
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IAttachmentRepository : IGenericRepository<Attachment>
    {
        Task<List<Attachment>> GetByItemIdAsync(Guid itemId);
        Task AddRangeAsync(IEnumerable<Attachment> attachments);
    }
}