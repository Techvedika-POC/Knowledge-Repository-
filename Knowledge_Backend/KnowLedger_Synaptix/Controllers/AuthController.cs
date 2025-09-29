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
        
>>>>>>> d254d38a840fa3205332ee1ad654f1c0b600c1bf
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
       
>>>>>>> d254d38a840fa3205332ee1ad654f1c0b600c1bf
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
}
=======
}
>>>>>>> d254d38a840fa3205332ee1ad654f1c0b600c1bf
