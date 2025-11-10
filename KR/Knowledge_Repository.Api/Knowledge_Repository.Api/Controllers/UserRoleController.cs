using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Knowledge_Repository.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserRoleController : ControllerBase
    {
        private readonly IUserRoleService _userRoleService;

        public UserRoleController(IUserRoleService userRoleService)
        {
            _userRoleService = userRoleService;
        }

        /// <summary>
        /// Gets all roles assigned to a specific user.
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetRolesForUser(Guid userId)
        {
            var roles = await _userRoleService.GetRolesForUserAsync(userId);
            if (roles == null || !roles.Any())
                return NotFound(new { Message = "No roles found for this user." });

            return Ok(roles);
        }

        /// <summary>
        /// Assigns a role to a user.
        /// </summary>
        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
            if (request == null || request.UserId == Guid.Empty || request.RoleId == Guid.Empty)
                return BadRequest(new { Message = "Invalid user or role ID." });

            await _userRoleService.AssignRoleToUserAsync(request.UserId, request.RoleId);
            return Ok(new { Message = "Role assigned successfully." });
        }

        /// <summary>
        /// Removes a role from a user.
        /// </summary>
        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveRole([FromBody] RemoveRoleRequest request)
        {
            if (request == null || request.UserId == Guid.Empty || request.RoleId == Guid.Empty)
                return BadRequest(new { Message = "Invalid user or role ID." });

            await _userRoleService.RemoveRoleFromUserAsync(request.UserId, request.RoleId);
            return Ok(new { Message = "Role removed successfully." });
        }
    }

   
    public class AssignRoleRequest
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
    }

    public class RemoveRoleRequest
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
    }
}
