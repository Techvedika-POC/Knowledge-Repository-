using Knowledge_Repository.Application.Dtos;
namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IAuthService
    {
        
        Task<bool> RegisterAsync(RegisterDto dto, Guid? createdBy = null);

        Task<AuthResponseDto?> LoginAsync(LoginDto dto);


    }
}
