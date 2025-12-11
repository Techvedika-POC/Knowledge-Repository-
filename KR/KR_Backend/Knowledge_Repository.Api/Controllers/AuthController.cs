using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Knowledge_Repository.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IPasswordResetService _passwordResetService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            IPasswordResetService passwordResetService,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _passwordResetService = passwordResetService;
            _configuration = configuration;
            _logger = logger;
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

            return Ok(user);
        }

        /// <summary>
        /// Start forgot-password flow
        /// </summary>
        [HttpPost("forgot")]
        public async Task<IActionResult> Forgot([FromBody] ForgotRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { message = "Email is required." });

            await _passwordResetService.ForgotPasswordAsync(request);


            return Ok(new { message = "If an account with this email exists, an OTP has been sent." });
        }

        /// <summary>
        /// Verify OTP – service issues reset token. By default token is emailed or logged.
        /// </summary>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
                return BadRequest(new { message = "Email and Code are required." });

            var resetToken = await _passwordResetService.VerifyOtpAsync(request);

            if (resetToken == null)
                return BadRequest(new { message = "Invalid or expired OTP." });

            // optionally return token in response (DEV only) via config
            var returnToken = bool.TryParse(_configuration["PasswordReset:ReturnTokenInResponse"], out var rt) && rt;
            if (returnToken)
                return Ok(new { message = "OTP verified.", resetToken });

            return Ok(new { message = "OTP verified. A reset token has been issued to the account email." });
        }

        /// <summary>
        /// Reset password using reset token (plaintext).
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email)
                || string.IsNullOrWhiteSpace(request.ResetToken) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { message = "Email, ResetToken and NewPassword are required." });
            }

            var ok = await _passwordResetService.ResetPasswordAsync(request);
            if (!ok)
                return BadRequest(new { message = "Invalid or expired reset token, or unable to reset password." });

            return Ok(new { message = "Password has been reset successfully." });
        }
        [HttpGet("test-email")]
        public async Task<IActionResult> TestEmail([FromServices] IEmailService emailService)
        {
            try
            {
                await emailService.SendEmailAsync("sangatiprameela597@gmail.com", "SMTP Test", "<p>Test email from API</p>");
                return Ok(new { message = "Email send attempt completed." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Send failed", error = ex.Message, detail = ex.ToString() });
            }
        }


    }
}
