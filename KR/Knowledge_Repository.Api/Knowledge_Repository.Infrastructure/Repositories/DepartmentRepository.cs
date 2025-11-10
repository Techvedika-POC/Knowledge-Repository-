using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public DepartmentRepository(Knowledge_Repository_dbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Department?> GetByNameAsync(string departmentName)
        {
            return await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentName.ToLower() == departmentName.ToLower());
        }
    }
}
