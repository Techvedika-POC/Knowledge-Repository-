using System.Collections.Generic;
using System.Threading.Tasks;
using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IDepartmentService
    {
        /// <summary>
        /// get all departments available
        /// </summary>
        Task<IEnumerable<Department>> GetDepartmentsAsync();
    }
}
