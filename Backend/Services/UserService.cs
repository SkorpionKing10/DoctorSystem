using Backend.Model;
using Backend.Repositories;

namespace Backend.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> GetByIdAsync(int id)
        => await _userRepository.GetByIdAsync(id);

    public async Task<User?> GetByUsernameAsync(string username)
        => await _userRepository.GetByUsernameAsync(username);

    public async Task<User?> GetByDomainUsernameAsync(string domainUsername)
        => await _userRepository.GetByDomainUsernameAsync(domainUsername);

    public async Task<List<User>> GetAllAsync()
        => await _userRepository.GetAllAsync();

    public async Task<User> CreateAsync(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            throw new ArgumentException("Benutzername und Passwort sind erforderlich.");

        if (!Enum.TryParse<UserRole>(dto.Role, ignoreCase: true, out var role))
            throw new ArgumentException("Ungültige Rolle. Erlaubt: Admin, Doctor, Staff");

        if (await _userRepository.ExistsAsync(dto.Username))
            throw new InvalidOperationException("Benutzername bereits vergeben.");

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = dto.Password,
            Role = role,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        return await _userRepository.CreateAsync(user);
    }

    public async Task<User> UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"User mit ID {id} nicht gefunden.");

        user.Username = dto.Username;
        user.Role = dto.Role;
        user.IsActive = dto.IsActive;

        return await _userRepository.UpdateAsync(user);
    }

    public async Task DeleteAsync(int id)
        => await _userRepository.DeleteAsync(id);
}