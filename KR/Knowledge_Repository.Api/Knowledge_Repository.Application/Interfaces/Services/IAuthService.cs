using Knowledge_Repository.Application.Dtos;
namespace Knowledge_Repository.Application.Interfaces.Services
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
