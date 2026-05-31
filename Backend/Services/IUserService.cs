using Backend.Model;

namespace Backend.Services;

public interface IUserService
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByDomainUsernameAsync(string domainUsername);
    Task<List<User>> GetAllAsync();
    Task<User> CreateAsync(CreateUserDto dto);
    Task<User> UpdateAsync(int id, UpdateUserDto dto);
    Task DeleteAsync(int id);
}