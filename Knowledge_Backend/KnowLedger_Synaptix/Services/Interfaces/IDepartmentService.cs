using System.Collections.Generic;
using System.Threading.Tasks;
using KnowLedger_Synaptix.Dtos;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync();
    }
}
