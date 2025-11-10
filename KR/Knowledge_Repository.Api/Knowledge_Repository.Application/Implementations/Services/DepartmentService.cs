using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    /// <summary>
    /// Service for managing department operations.
    /// </summary>
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentService(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        /// <summary>
        /// Fetches all departments efficiently with projection.
        /// </summary>
        public async Task<IEnumerable<Department>> GetDepartmentsAsync()
        {
            var departments = await _departmentRepository.GetAllAsync();

            return departments
                .Select(d => new Department
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    Description = d.Description
                })
                .ToList();
        }
    }
}
