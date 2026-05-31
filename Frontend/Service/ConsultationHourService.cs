using Frontend.Models;
using System.Net.Http.Json;

namespace Frontend.Services;

public class ConsultationHourService : IConsultationHourService
{
    private readonly HttpClient _http;

    public ConsultationHourService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ConsultationHourDto>> GetAllAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<ConsultationHourDto>>("api/consultation-hours") ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<ConsultationHourDto?> GetByIdAsync(int id)
    {
        try
        {
            var all = await GetAllAsync();
            return all.FirstOrDefault(c => c.Id == id);
        }
        catch
        {
            return null;
        }
    }
}