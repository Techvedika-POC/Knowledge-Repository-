using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IKnowledgeVersionRepository:IGenericRepository<KnowledgeVersion>
    {
        Task<KnowledgeVersion?> GetLastVersionByItemIdAsync(Guid itemId);
    }
}
