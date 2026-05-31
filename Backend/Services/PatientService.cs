using Backend.Model;
using Backend.Repositories;

namespace Backend.Services;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;

    public PatientService(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<Patient?> GetByIdAsync(int id)
        => await _patientRepository.GetByIdAsync(id);

    public async Task<Patient?> GetByUserIdAsync(int userId)
        => await _patientRepository.GetByUserIdAsync(userId);

    public async Task<List<Patient>> GetAllAsync()
        => await _patientRepository.GetAllAsync();

    public async Task<Patient> CreateAsync(Patient patient)
        => await _patientRepository.CreateAsync(patient);

    public async Task<Patient> UpdateAsync(int id, Patient patient)
    {
        var existing = await _patientRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Patient mit ID {id} nicht gefunden.");

        existing.FirstName = patient.FirstName;
        existing.LastName = patient.LastName;
        existing.BirthDate = patient.BirthDate;
        existing.SocialSecurityNumber = patient.SocialSecurityNumber;

        return await _patientRepository.UpdateAsync(existing);
    }

    public async Task DeleteAsync(int id)
        => await _patientRepository.DeleteAsync(id);
}