using Frontend.Models;
using System.Net.Http.Json;

namespace Frontend.Services;

public class AppointmentService : IAppointmentService
{
    private readonly HttpClient _http;

    public AppointmentService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<AppointmentDto>> GetAllAppointmentsAsync()
    {
        try { return await _http.GetFromJsonAsync<List<AppointmentDto>>("api/appointments") ?? new(); }
        catch { return new(); }
    }

    public async Task<List<AppointmentDto>> GetPatientAppointmentsAsync(int patientId)
    {
        try { return await _http.GetFromJsonAsync<List<AppointmentDto>>($"api/appointments/patient/{patientId}") ?? new(); }
        catch { return new(); }
    }

    public async Task<List<AppointmentDto>> GetMyAppointmentsAsync()
    {
        try { return await _http.GetFromJsonAsync<List<AppointmentDto>>("api/appointments/my-appointments") ?? new(); }
        catch { return new(); }
    }

    public async Task<List<string>> GetFreeSlotsAsync(int consultationHourId, DateTime date)
    {
        try
        {
            var dateStr = date.ToString("yyyy-MM-dd");
            return await _http.GetFromJsonAsync<List<string>>($"api/appointments/free-slots/{consultationHourId}/{dateStr}") ?? new();
        }
        catch { return new(); }
    }

    public async Task<AppointmentDto?> CreateAppointmentAsync(AppointmentCreateDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/appointments", dto);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<AppointmentDto>(); // ✓ fix
        }
        catch { }
        return null;
    }

    public async Task<bool> CancelAppointmentAsync(int id)
    {
        try
        {
            var response = await _http.PostAsync($"api/appointments/cancel/{id}", null);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> UpdateAppointmentAsync(int id, AppointmentDto appointment)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/appointments/{id}", appointment);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> DeleteAppointmentAsync(int id)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/appointments/{id}");
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<AppointmentDto?> BookNextAvailableAsync(int patientId, int consultationHourId)
    {
        try
        {
            var response = await _http.PostAsync($"api/appointments/book?patientId={patientId}&consultationHourId={consultationHourId}", null);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<AppointmentDto>(); // ✓ fix
        }
        catch { }
        return null;
    }
}