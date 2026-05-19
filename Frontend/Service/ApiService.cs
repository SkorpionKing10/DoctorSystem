using Frontend.Models;
using System.Net.Http.Json;

namespace Frontend.Services;

public class ApiService
{
    private readonly HttpClient _http;

    public ApiService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient();
        _http.BaseAddress = new Uri("http://localhost:5264/");
    }

    public async Task<List<AppointmentDto>> GetAppointments()
    {
        return await _http.GetFromJsonAsync<List<AppointmentDto>>("api/appointments")
               ?? new List<AppointmentDto>();
    }
}