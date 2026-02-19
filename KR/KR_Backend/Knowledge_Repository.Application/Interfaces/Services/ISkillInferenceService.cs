using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface ISkillInferenceService
    {
        Task InferFromActivityAsync(
            Guid userId,
            string activityContext
        );
    }
}
