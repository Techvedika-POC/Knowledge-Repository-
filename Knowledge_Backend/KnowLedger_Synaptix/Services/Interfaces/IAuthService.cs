using KnowLedger_Synaptix.Dtos;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterDto dto, Guid? createdBy = null);
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);


    }
}
