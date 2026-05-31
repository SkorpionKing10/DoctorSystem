using Frontend.Models;
using System.Net.Http.Json;

namespace Frontend.Services;

public class PatientService : IPatientService
{
    private readonly HttpClient _http;

    public PatientService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<PatientDto>> GetAllPatientsAsync()
    {
        try { return await _http.GetFromJsonAsync<List<PatientDto>>("api/patients") ?? new(); }
        catch { return new(); }
    }

    public async Task<PatientDto?> GetPatientByIdAsync(int id)
    {
        try { return await _http.GetFromJsonAsync<PatientDto>($"api/patients/{id}"); }
        catch { return null; }
    }

    public async Task<PatientLookupDto?> GetMyPatientAsync(string username)
    {
        try { return await _http.GetFromJsonAsync<PatientLookupDto>($"api/patients/by-username/{username}"); }
        catch { return null; }
    }

    public async Task<PatientDto?> CreatePatientAsync(PatientDto patient)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/patients", patient);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<PatientDto>(); // ✓ fix
        }
        catch { }
        return null;
    }

    public async Task<bool> UpdatePatientAsync(int id, PatientDto patient)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/patients/{id}", patient);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> DeletePatientAsync(int id)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/patients/{id}");
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}