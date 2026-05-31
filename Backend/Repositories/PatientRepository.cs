using Backend.Model;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly DoctorDbContext _db;

    public PatientRepository(DoctorDbContext db)
    {
        _db = db;
    }

    public async Task<Patient?> GetByIdAsync(int id)
        => await _db.Patients.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Patient?> GetByUserIdAsync(int userId)
        => await _db.Patients.FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task<List<Patient>> GetAllAsync()
        => await _db.Patients.ToListAsync();

    public async Task<Patient> CreateAsync(Patient patient)
    {
        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();
        return patient;
    }

    public async Task<Patient> UpdateAsync(Patient patient)
    {
        _db.Patients.Update(patient);
        await _db.SaveChangesAsync();
        return patient;
    }

    public async Task DeleteAsync(int id)
    {
        var patient = await _db.Patients.FindAsync(id);
        if (patient != null)
        {
            _db.Patients.Remove(patient);
            await _db.SaveChangesAsync();
        }
    }
}