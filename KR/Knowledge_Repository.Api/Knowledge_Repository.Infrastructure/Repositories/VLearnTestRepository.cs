using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Infrastructure.Data;
using KnowLedger_Synaptix.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class VLearnTestRepository : IVLearnTestRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public VLearnTestRepository(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Dynamically generates questions based on a topic or module.
        /// </summary>
        /// <param name="request">VLearnQuestionRequestDto with module name or context type.</param>
        /// <returns>Returns a DTO containing a list of questions and options.</returns>
        public async Task<VLearnQuestionResponseDto> GenerateQuestionsAsync(VLearnQuestionRequestDto request)
        {

            var questionsQuery = _context.KnowledgeItems
                .Where(k => k.Title.Contains(request.ModuleName));

            var data = await questionsQuery
                .Select(k => new VLearnQuestionDto
                {
                    Question = $"What is related to {k.Title}?",
                    Options = new List<string>
                    {
                        "Option A for " + k.Title,
                        "Option B for " + k.Title,
                        "Option C for " + k.Title,
                        "Option D for " + k.Title
                    },
                    CorrectAnswer = "Option A for " + k.Title 
                })
                .Take(10)
                .ToListAsync();

            return new VLearnQuestionResponseDto
            {
                Questions = data
            };
        }
    }
}
