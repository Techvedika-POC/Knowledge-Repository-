using Knowledge_Repository.Application.Dtos.JuryCommunication;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IJuryFinalScoreService
    {
        Task<Guid> SubmitFinalScoreAsync(FinalScoreDto dto);
    }
}
