using Backend.Model;

namespace Backend.Repositories;

public interface IDoctorRepository
{
    Task<Doctor?> GetByIdAsync(int id);
    Task<List<Doctor>> GetAllAsync();
    Task<Doctor> CreateAsync(Doctor doctor);
    Task<Doctor> UpdateAsync(Doctor doctor);
    Task DeleteAsync(int id);
}