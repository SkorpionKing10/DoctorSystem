using Frontend.Models;

namespace Frontend.Services;

public interface IPatientService
{
    Task<List<PatientDto>> GetAllPatientsAsync();
    Task<PatientDto?> GetPatientByIdAsync(int id);
    Task<PatientLookupDto?> GetMyPatientAsync(string username);
    Task<PatientDto?> CreatePatientAsync(PatientDto patient);
    Task<bool> UpdatePatientAsync(int id, PatientDto patient);
    Task<bool> DeletePatientAsync(int id);
}