using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface ILearningEventService
    {
        Task LogAndProcessAsync(
            Guid userId,
            string eventType,
            string entityType,
            Guid? entityId,
            string? metadata = null);
    }

}
