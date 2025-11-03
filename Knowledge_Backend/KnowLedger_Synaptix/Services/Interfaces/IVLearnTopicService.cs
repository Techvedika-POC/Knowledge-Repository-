using System.Collections.Generic;
using System.Threading.Tasks;
using KnowLedger_Synaptix.Models;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IVLearnTopicService
    {
        Task<IEnumerable<Topic>> GetAllTopicsAsync();
    }
}
