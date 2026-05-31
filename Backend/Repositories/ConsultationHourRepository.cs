using Backend.Model;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class ConsultationHourRepository : IConsultationHourRepository
{
    private readonly DoctorDbContext _db;

    public ConsultationHourRepository(DoctorDbContext db)
    {
        _db = db;
    }

    public async Task<ConsultationHour?> GetByIdAsync(int id)
        => await _db.ConsultationHours.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<List<ConsultationHour>> GetAllAsync()
        => await _db.ConsultationHours.Where(c => c.IsActive).ToListAsync();

    public async Task<ConsultationHour> CreateAsync(ConsultationHour consultationHour)
    {
        _db.ConsultationHours.Add(consultationHour);
        await _db.SaveChangesAsync();
        return consultationHour;
    }

    public async Task<ConsultationHour> UpdateAsync(ConsultationHour consultationHour)
    {
        _db.ConsultationHours.Update(consultationHour);
        await _db.SaveChangesAsync();
        return consultationHour;
    }

    public async Task DeleteAsync(int id)
    {
        var consultationHour = await _db.ConsultationHours.FindAsync(id);
        if (consultationHour != null)
        {
            _db.ConsultationHours.Remove(consultationHour);
            await _db.SaveChangesAsync();
        }
    }
}