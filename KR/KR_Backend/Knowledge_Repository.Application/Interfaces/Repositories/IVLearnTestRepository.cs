using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
 
    public interface IVLearnTestRepository
    {
        Task<VLearnQuestionResponseDto> GenerateQuestionsAsync(VLearnQuestionRequestDto request);
    }

}
