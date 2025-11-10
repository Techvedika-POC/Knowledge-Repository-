using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Knowledge_Repository.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        // Public endpoint (no auth)
        [HttpGet("public")]
        public IActionResult Public() => Ok("Public endpoint works!");

        // Secured endpoint (requires JWT)
        [HttpGet("jwt")]
        [Authorize]
        public IActionResult JwtProtected()
        {
            // Return basic info from token
            var userId = User.FindFirst("nameid")?.Value ?? "unknown";
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "unknown";

            return Ok(new
            {
                message = "JWT is valid!",
                userId,
                email
            });
        }
    }
}
