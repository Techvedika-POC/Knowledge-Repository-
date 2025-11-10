using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Knowledge_Repository.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Role>>> GetAllRoles()
        {
            return Ok(await _roleService.GetAllRolesAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRoleById(Guid id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null) return NotFound();
            return Ok(role);
        }

        [HttpPost]
        public async Task<ActionResult> CreateRole([FromBody] Role role)
        {
            var created = await _roleService.CreateRoleAsync(role.RoleName, role.Description);
            return Ok(created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateRole(Guid id, [FromBody] Role role)
        {
            var updated = await _roleService.UpdateRoleAsync(id, role.RoleName, role.Description);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRole(Guid id)
        {
            await _roleService.DeleteRoleAsync(id);
            return Ok(new { message = "Role deleted successfully." });
        }
    }
}
