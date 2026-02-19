using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IKnowledgeTagRepository : IGenericRepository<KnowledgeTag>
    {
        Task AddRangeAsync(IEnumerable<KnowledgeTag> tags);
        Task<List<KnowledgeTag>> GetTagsByItemIdAsync(Guid itemId);
        Task<List<KnowledgeTag>> GetByItemAndVersionAsync(Guid itemId, Guid versionId);

    }
}
