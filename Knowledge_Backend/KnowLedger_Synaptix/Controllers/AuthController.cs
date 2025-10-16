using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KnowLedger_Synaptix.Controllers
{
    /// <summary>
    /// Handles user authentication and registration processes.
    /// Provides endpoints for new user registration and login.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // Validate request body
            if (dto == null)
                return BadRequest("Invalid request.");

            var result = await _authService.RegisterAsync(dto);

            if (!result)
                return BadRequest("User with this email already exists.");

            return Ok("User registered successfully.");
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token upon successful login.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            // Validate input
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { message = "Email and password are required." });

            // Authenticate user
            var user = await _authService.LoginAsync(request);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });

            // Return token and user info on success
            return Ok(new
            {
                token = user.Token,
                name = user.Name,
                email = user.Email,
                roles = user.Roles,
                expires = DateTime.UtcNow.AddMinutes(60)
            });
        }
    }
}
