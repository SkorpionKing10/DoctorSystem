using Frontend.Models;
using System.Net.Http.Json;

namespace Frontend.Services;

public class ApiService
{
    private readonly HttpClient _http;

    public ApiService(IHttpClientFactory factory)
    {
        var handler = new HttpClientHandler
        {
            UseDefaultCredentials = true  // ← Kerberos
        };
        _http = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://192.168.68.50:5000/")
        };
    }

    public async Task<List<AppointmentDto>> GetAppointments()
    {
        return await _http.GetFromJsonAsync<List<AppointmentDto>>("api/appointments")
               ?? new();
    }
}