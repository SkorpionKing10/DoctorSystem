using Microsoft.JSInterop;

namespace Frontend.Services;

public class AuthService
{
    private readonly IJSRuntime _js;

    public bool IsLoggedIn { get; private set; }
    public string? Username { get; private set; }
    public string? Role { get; private set; }
    public bool IsAdmin => Role == "0";

    public AuthService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync()
    {
        var username = await _js.InvokeAsync<string?>("localStorage.getItem", "username");
        var role = await _js.InvokeAsync<string?>("localStorage.getItem", "role");
        if (!string.IsNullOrEmpty(username))
        {
            IsLoggedIn = true;
            Username = username;
            Role = role;
        }
    }

    public async Task LoginAsync(string username, string role)
    {
        IsLoggedIn = true;
        Username = username;
        Role = role;
        await _js.InvokeVoidAsync("localStorage.setItem", "username", username);
        await _js.InvokeVoidAsync("localStorage.setItem", "role", role);
    }

    public async Task LogoutAsync()
    {
        IsLoggedIn = false;
        Username = null;
        Role = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", "username");
        await _js.InvokeVoidAsync("localStorage.removeItem", "role");
    }
}