using Backend.Model;

namespace Backend.Services;

public interface IPatientService
{
    Task<Patient?> GetByIdAsync(int id);
    Task<Patient?> GetByUserIdAsync(int userId);
    Task<List<Patient>> GetAllAsync();
    Task<Patient> CreateAsync(Patient patient);
    Task<Patient> UpdateAsync(int id, Patient patient);
    Task DeleteAsync(int id);
}