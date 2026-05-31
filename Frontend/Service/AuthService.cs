using Frontend.Models;
using System.Net.Http.Json;

namespace Frontend.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _http;

    public bool IsLoggedIn { get; private set; }
    public string? Username { get; private set; }
    public string? Role { get; private set; }

    public bool IsAdmin => Role == "Admin";
    public bool IsDoctor => Role == "Doctor";
    public bool IsStaff => Role == "Staff";

    public AuthService(HttpClient http)
    {
        _http = http;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<AuthResponse>("api/auth/me");

            if (result != null)
            {
                IsLoggedIn = true;
                Username = result.Username;
                Role = result.Role;
            }
        }
        catch
        {
            IsLoggedIn = false;
        }
    }

    public async Task LogoutAsync()
    {
        IsLoggedIn = false;
        Username = null;
        Role = null;
        await Task.CompletedTask;
    }
}