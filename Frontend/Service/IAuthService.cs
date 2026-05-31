using Frontend.Models;

namespace Frontend.Services;

public interface IAuthService
{
    bool IsLoggedIn { get; }
    string? Username { get; }
    string? Role { get; }
    bool IsAdmin { get; }
    bool IsDoctor { get; }
    bool IsStaff { get; }

    Task InitializeAsync();
    Task LogoutAsync();
}