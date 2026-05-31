using Backend.Model;

namespace Backend.Services;

public interface IDoctorService
{
    Task<Doctor?> GetByIdAsync(int id);
    Task<List<Doctor>> GetAllAsync();
    Task<Doctor> CreateAsync(Doctor doctor);
    Task<Doctor> UpdateAsync(int id, Doctor doctor);
    Task DeleteAsync(int id);
}