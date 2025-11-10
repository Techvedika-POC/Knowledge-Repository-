using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Knowledge_Repository.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Get a user by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            return Ok(user);
        }

        /// <summary>
        /// Get a user (with roles) by email
        /// </summary>
        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var user = await _userService.GetUserWithRolesAsync(email);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            return Ok(user);
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] User user)
        {
            if (await _userService.ExistsByEmailAsync(user.Email))
                return Conflict(new { Message = "Email already exists." });

            await _userService.AddUserAsync(user);
            return Ok(new { Message = "User created successfully" });
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] User user)
        {
            if (id != user.UserId)
                return BadRequest(new { Message = "User ID mismatch." });

            await _userService.UpdateUserAsync(user);
            return Ok(new { Message = "User updated successfully" });
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _userService.DeleteUserAsync(id);
            return Ok(new { Message = "User deleted successfully" });
        }
    }
}
