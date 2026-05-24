using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace Frontend.Services;

public class AuthService
{
    private readonly HttpClient _http;

    public bool IsLoggedIn { get; private set; }
    public string? Username { get; private set; }
    public string? Role { get; private set; }

    // Rollen kommen jetzt als String vom Backend: "Admin", "Doctor", "Staff"
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
            // Kerberos-Ticket wird automatisch mitgeschickt
            var result = await _http.GetFromJsonAsync<MeResponse>(
                "http://192.168.68.50:5000/api/auth/me");

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

    // Nicht mehr nötig bei Kerberos – bleibt leer für Kompatibilität
    public Task LogoutAsync()
    {
        IsLoggedIn = false;
        Username = null;
        Role = null;
        return Task.CompletedTask;
    }

    class MeResponse
    {
        public string Username { get; set; } = "";
        public string Role { get; set; } = "";
    }
}