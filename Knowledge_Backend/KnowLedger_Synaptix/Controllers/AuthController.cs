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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { message = "Email and password are required" });

            var user = await _authService.LoginAsync(request);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            //return Ok(new
            //{
            //    token = user.Token,
            //    name = user.Name,
            //    email = user.Email,
            //    roles = user.Roles,
            //    expires = DateTime.UtcNow.AddMinutes(60)
            //});
            return Ok(user);
        }
    }
}


