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
        try
        {
            Console.WriteLine("GetAllPatientsAsync: Requesting from api/patients");
            var response = await _http.GetAsync("api/patients");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"GetAllPatientsAsync Error: {response.StatusCode}");
                return new();
            }

            var patients = await response.Content.ReadFromJsonAsync<List<PatientDto>>() ?? new();
            Console.WriteLine($"GetAllPatientsAsync Success: {patients.Count} patients loaded");
            return patients;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetAllPatientsAsync Exception: {ex.Message}");
            return new();
        }
    }

    public async Task<PatientDto?> GetPatientByIdAsync(int id)
    {
        try
        {
            var response = await _http.GetAsync($"api/patients/{id}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"GetPatientByIdAsync Error: {response.StatusCode}");
                return null;
            }
            return await response.Content.ReadFromJsonAsync<PatientDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetPatientByIdAsync Exception: {ex.Message}");
            return null;
        }
    }

    public async Task<PatientLookupDto?> GetMyPatientAsync(string username)
    {
        try
        {
            var response = await _http.GetAsync($"api/patients/by-username/{username}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"GetMyPatientAsync Error: {response.StatusCode}");
                return null;
            }
            return await response.Content.ReadFromJsonAsync<PatientLookupDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetMyPatientAsync Exception: {ex.Message}");
            return null;
        }
    }

    public async Task<PatientDto?> CreatePatientAsync(PatientDto patient)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/patients", patient);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"CreatePatientAsync Error: {response.StatusCode} - {error}");
                return null;
            }
            return await response.Content.ReadFromJsonAsync<PatientDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CreatePatientAsync Exception: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdatePatientAsync(int id, PatientDto patient)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/patients/{id}", patient);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"UpdatePatientAsync Error: {response.StatusCode} - {error}");
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UpdatePatientAsync Exception: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeletePatientAsync(int id)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/patients/{id}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"DeletePatientAsync Error: {response.StatusCode}");
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DeletePatientAsync Exception: {ex.Message}");
            return false;
        }
    }
}