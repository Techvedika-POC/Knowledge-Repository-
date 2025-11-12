using Knowledge_Repository.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IDepartmentService
    {
       
        Task<IEnumerable<Department>> GetDepartmentsAsync();
    }
}
