using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KnowLedger_Synaptix.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
<<<<<<< HEAD


=======
>>>>>>> d28a140357eeb67798a19d51849f1d88a3c8c9a7
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid request.");

            var result = await _authService.RegisterAsync(dto);

            if (!result)
                return BadRequest("User with this email already exists.");

            return Ok("User registered successfully.");
        }

<<<<<<< HEAD

=======
>>>>>>> d28a140357eeb67798a19d51849f1d88a3c8c9a7
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid request.");

            var authResponse = await _authService.LoginAsync(dto);

            if (authResponse == null)
                return Unauthorized("Invalid email or password.");

            return Ok(authResponse);
        }
    }
<<<<<<< HEAD

=======
>>>>>>> d28a140357eeb67798a19d51849f1d88a3c8c9a7
}
