using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IPasswordResetRepository _passwordResetRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<PasswordResetService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService; 

        public PasswordResetService(
            IPasswordResetRepository passwordResetRepository,
            IUserRepository userRepository,
            ILogger<PasswordResetService> logger,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _passwordResetRepository = passwordResetRepository;
            _userRepository = userRepository;
            _logger = logger;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<bool> ForgotPasswordAsync(ForgotRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
                return false;

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogInformation("ForgotPassword requested for unknown email {Email}", request.Email);
                return true;
            }


            var otp = new Random().Next(100000, 999999).ToString();
            var otpHash = BCryptNet.HashPassword(otp);

            var otpExpiryMinutes = int.TryParse(_configuration["PasswordReset:OtpExpiryMinutes"], out var m) ? m : 10;

            var pr = new PasswordReset
            {
                UserId = user.UserId,
                CodeHash = otpHash,
                CodeExpiresAt = DateTime.UtcNow.AddMinutes(otpExpiryMinutes),
                CodeUsed = false,
                CreatedOn = DateTime.UtcNow
            };

            await _passwordResetRepository.AddAsync(pr);

    
            var htmlBody = $"<p>Your password reset OTP is <strong>{otp}</strong>. It expires in {otpExpiryMinutes} minutes.</p>";
            try
            {
                if (_emailService != null)
                {
                    await _emailService.SendEmailAsync(user.Email, "Password reset OTP", htmlBody);
                }
                else
                {
                    _logger.LogInformation("OTP for {Email}: {Otp} (expires in {Minutes} minutes). No email service configured.", user.Email, otp, otpExpiryMinutes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {Email}", user.Email);
                
            }

            return true;
        }

        public async Task<string?> VerifyOtpAsync(VerifyOtpDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
                return null;

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null) return null;

            var pr = await _passwordResetRepository.GetLatestByUserIdAsync(user.UserId);
            if (pr == null) return null;
            if (pr.CodeUsed || pr.CodeExpiresAt < DateTime.UtcNow) return null;

            if (!BCryptNet.Verify(request.Code, pr.CodeHash)) return null;

            pr.CodeUsed = true;

    
            var resetToken = Guid.NewGuid().ToString("N") + "-" + Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=');
            pr.ResetTokenHash = BCryptNet.HashPassword(resetToken);
            pr.ResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(int.TryParse(_configuration["PasswordReset:ResetTokenExpiryMinutes"], out var r) ? r : 60);
            pr.ResetTokenUsed = false;
            pr.UpdatedOn = DateTime.UtcNow;

            await _passwordResetRepository.UpdateAsync(pr);

    
            var html = $"<p>Your password reset token is:<br/><code>{resetToken}</code><br/>It expires at {pr.ResetTokenExpiresAt:O} (UTC).</p>";
            try
            {
                if (_emailService != null)
                {
                    await _emailService.SendEmailAsync(user.Email, "Password reset token", html);
                }
                else
                {
                    _logger.LogInformation("Reset token for {Email}: {Token} (expires {Expiry})", user.Email, resetToken, pr.ResetTokenExpiresAt);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reset token email to {Email}", user.Email);
            }

  
            return resetToken;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.ResetToken) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
                return false;

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null) return false;

            var pr = await _passwordResetRepository.GetLatestByUserIdAsync(user.UserId);
            if (pr == null) return false;

            if (pr.ResetTokenUsed) return false;
            if (!pr.ResetTokenExpiresAt.HasValue || pr.ResetTokenExpiresAt.Value < DateTime.UtcNow) return false;
            if (string.IsNullOrEmpty(pr.ResetTokenHash)) return false;

            if (!BCryptNet.Verify(request.ResetToken, pr.ResetTokenHash)) return false;

            user.PasswordHash = BCryptNet.HashPassword(request.NewPassword);
            user.UpdatedOn = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            pr.ResetTokenUsed = true;
            pr.UpdatedOn = DateTime.UtcNow;
            await _passwordResetRepository.UpdateAsync(pr);

            _logger.LogInformation("Password reset completed for {Email}", user.Email);
            return true;
        }
    }
}
