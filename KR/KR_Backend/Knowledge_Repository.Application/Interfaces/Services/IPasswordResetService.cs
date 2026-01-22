using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IPasswordResetService
    {
        Task<bool> ForgotPasswordAsync(ForgotRequestDto request);
        Task<string?> VerifyOtpAsync(VerifyOtpDto request);
        Task<bool> ResetPasswordAsync(ResetPasswordDto request);
    }
}
