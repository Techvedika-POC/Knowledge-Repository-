using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IUserLessonProgressRepository
    {
        Task<UserLessonProgress?> GetAsync(Guid userId, Guid lessonId);
        Task StartAsync(Guid userId, Guid lessonId, Guid moduleId);
        Task CompleteAsync(Guid userId, Guid lessonId);
        Task<List<UserLessonProgress>> GetByModuleAsync(Guid userId, Guid moduleId);
    }


}
