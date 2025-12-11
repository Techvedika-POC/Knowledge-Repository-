using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IPasswordResetService
    {
        /// <summary>
        /// Starts forgot password flow: generate OTP, store hashed OTP and expiry, optionally email it.
        /// Returns true even if email not found (to avoid account enumeration).
        /// </summary>
        Task<bool> ForgotPasswordAsync(ForgotRequestDto request);

        /// <summary>
        /// Verify OTP; if valid issues a reset token and returns the plaintext token (service may email it instead).
        /// Returns null when invalid/expired.
        /// </summary>
        Task<string?> VerifyOtpAsync(VerifyOtpDto request);

        /// <summary>
        /// Reset password using the plaintext reset token (previously issued).
        /// Returns true on success.
        /// </summary>
        Task<bool> ResetPasswordAsync(ResetPasswordDto request);
    }
}
