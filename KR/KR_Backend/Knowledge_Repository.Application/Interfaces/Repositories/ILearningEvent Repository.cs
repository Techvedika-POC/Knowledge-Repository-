using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface ILearningEventRepository
    {
        Task LogAsync(
            Guid userId,
            string eventType,
            string entityType,
            Guid? entityId,
            string? metadata = null);
    }

}
