using KnowLedger_Synaptix.Dtos;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        Task<bool> RegisterAsync(RegisterDto dto, Guid? createdBy = null);

        /// <summary>
        /// Authenticates a user and returns authentication details.
        /// </summary>
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);


    }
}
