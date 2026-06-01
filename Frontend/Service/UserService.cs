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
        try
        {
            var response = await _http.GetAsync("api/users");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"GetAllUsersAsync Error: {response.StatusCode}");
                return new();
            }
            var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
            return users ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetAllUsersAsync Exception: {ex.Message}");
            return new();
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        try
        {
            var users = await GetAllUsersAsync();
            return users.FirstOrDefault(u => u.Id == id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetUserByIdAsync Exception: {ex.Message}");
            return null;
        }
    }

    public async Task<UserDto?> CreateUserAsync(CreateUserDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/users", dto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"CreateUserAsync Error: {response.StatusCode} - {error}");
                return null;
            }
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CreateUserAsync Exception: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        try
        {
            Console.WriteLine($"UpdateUserAsync: ID={id}, Username={dto.Username}, Role={dto.Role}, IsActive={dto.IsActive}");

            var response = await _http.PutAsJsonAsync($"api/users/{id}", dto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"UpdateUserAsync Error: {response.StatusCode} - {error}");
                return false;
            }

            Console.WriteLine("UpdateUserAsync Success");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UpdateUserAsync Exception: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/users/{id}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"DeleteUserAsync Error: {response.StatusCode}");
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DeleteUserAsync Exception: {ex.Message}");
            return false;
        }
    }
}