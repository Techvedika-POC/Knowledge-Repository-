using KnowLedger_Synaptix.Dtos;

namespace KnowLedger_Synaptix.Services.Interfaces
{
<<<<<<< HEAD
    public interface IAuthService
=======
    public interface  IAuthService
>>>>>>> d254d38a840fa3205332ee1ad654f1c0b600c1bf
    {
        Task<bool> RegisterAsync(RegisterDto dto, Guid? createdBy = null);
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);


    }
<<<<<<< HEAD
}
=======
}
>>>>>>> d254d38a840fa3205332ee1ad654f1c0b600c1bf
