using Backend.Model;
using Backend.Repositories;

namespace Backend.Services;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;

    public DoctorService(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<Doctor?> GetByIdAsync(int id)
        => await _doctorRepository.GetByIdAsync(id);

    public async Task<List<Doctor>> GetAllAsync()
        => await _doctorRepository.GetAllAsync();

    public async Task<Doctor> CreateAsync(Doctor doctor)
        => await _doctorRepository.CreateAsync(doctor);

    public async Task<Doctor> UpdateAsync(int id, Doctor doctor)
    {
        var existing = await _doctorRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Doctor mit ID {id} nicht gefunden.");

        existing.FirstName = doctor.FirstName;
        existing.LastName = doctor.LastName;
        existing.Title = doctor.Title;

        return await _doctorRepository.UpdateAsync(existing);
    }

    public async Task DeleteAsync(int id)
        => await _doctorRepository.DeleteAsync(id);
}