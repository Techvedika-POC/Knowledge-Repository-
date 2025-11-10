using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repository for handling VLearn tests and dynamic question generation.
    /// </summary>
    public interface IVLearnTestRepository
    {
        Task<VLearnQuestionResponseDto> GenerateQuestionsAsync(VLearnQuestionRequestDto request);
    }

}
