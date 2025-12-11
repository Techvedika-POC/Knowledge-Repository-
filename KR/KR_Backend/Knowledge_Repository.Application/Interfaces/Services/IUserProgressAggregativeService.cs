using Knowledge_Repository.Application.Dtos;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IUserProgressAggregateService
    {
        Task<UserPlanProgressDetailDto> GetUserPlanProgressAsync(Guid userId, Guid planId);
    }
}
