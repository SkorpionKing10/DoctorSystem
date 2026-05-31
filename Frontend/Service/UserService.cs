using Frontend.Models;
using System.Net.Http.Json;

namespace Frontend.Services;

public class UserService : IUserService
{
    private readonly HttpClient _http;

    public UserService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        try { return await _http.GetFromJsonAsync<List<UserDto>>("api/users") ?? new(); }
        catch { return new(); }
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        try
        {
            var users = await _http.GetFromJsonAsync<List<UserDto>>("api/users");
            return users?.FirstOrDefault(u => u.Id == id);
        }
        catch { return null; }
    }

    public async Task<UserDto?> CreateUserAsync(CreateUserDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/users", dto);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<UserDto>(); // ✓ fix
        }
        catch { }
        return null;
    }

    public async Task<bool> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/users/{id}", dto);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/users/{id}");
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}