using Knowledge_Repository.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IDepartmentService
    {
        /// <summary>
        /// Retrieves all departments in the system.
        /// </summary>
        Task<IEnumerable<Department>> GetDepartmentsAsync();
    }
}
