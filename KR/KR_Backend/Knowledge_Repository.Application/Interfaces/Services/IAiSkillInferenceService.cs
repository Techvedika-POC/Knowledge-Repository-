using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IAiSkillInferenceService
    {
        Task<Dictionary<string, double>> InferSkillsAsync(string context);
    }

}
