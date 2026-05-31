using Backend.Model;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly DoctorDbContext _db;

    public DoctorRepository(DoctorDbContext db)
    {
        _db = db;
    }

    public async Task<Doctor?> GetByIdAsync(int id)
        => await _db.Doctors.FirstOrDefaultAsync(d => d.Id == id);

    public async Task<List<Doctor>> GetAllAsync()
        => await _db.Doctors.ToListAsync();

    public async Task<Doctor> CreateAsync(Doctor doctor)
    {
        _db.Doctors.Add(doctor);
        await _db.SaveChangesAsync();
        return doctor;
    }

    public async Task<Doctor> UpdateAsync(Doctor doctor)
    {
        _db.Doctors.Update(doctor);
        await _db.SaveChangesAsync();
        return doctor;
    }

    public async Task DeleteAsync(int id)
    {
        var doctor = await _db.Doctors.FindAsync(id);
        if (doctor != null)
        {
            _db.Doctors.Remove(doctor);
            await _db.SaveChangesAsync();
        }
    }
}