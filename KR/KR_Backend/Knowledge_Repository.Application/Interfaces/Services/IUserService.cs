using Knowledge_Repository.Domain.Entities;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> ExistsByEmailAsync(string email);
        Task<User?> GetUserWithRolesAsync(string email);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(Guid userId);
        Task<bool> UpdateUserProfileAsync(Guid userId, UserProfileUpdateDto dto);

    }
}
