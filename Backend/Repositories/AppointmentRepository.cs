using Backend.Model;
using Microsoft.EntityFrameworkCore;
namespace Backend.Repositories;
public class AppointmentRepository : IAppointmentRepository
{
    private readonly DoctorDbContext _db;
    public AppointmentRepository(DoctorDbContext db)
    {
        _db = db;
    }
    public async Task<Appointment?> GetByIdAsync(int id)
        => await _db.Appointments.FirstOrDefaultAsync(a => a.Id == id);
    public async Task<List<Appointment>> GetAllAsync()
        => await _db.Appointments.ToListAsync();
    public async Task<List<Appointment>> GetByPatientIdAsync(int patientId)
        => await _db.Appointments
            .Where(a => a.PatientId == patientId && !a.IsCancelled)
            .OrderBy(a => a.Date)
            .ThenBy(a => a.Time)
            .ToListAsync();
    public async Task<List<Appointment>> GetByConsultationHourAsync(int consultationHourId, DateTime date)
        => await _db.Appointments
            .Where(a => a.ConsultationHourId == consultationHourId && a.Date.Date == date.Date && !a.IsCancelled)
            .ToListAsync();
    public async Task<List<Appointment>> GetPatientAppointmentsByDateAsync(int patientId, DateTime date)
        => await _db.Appointments
            .Where(a => a.PatientId == patientId && a.Date.Date == date.Date && !a.IsCancelled)
            .ToListAsync();
    public async Task<Appointment> CreateAsync(Appointment appointment)
    {
        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();
        return appointment;
    }
    public async Task<Appointment> UpdateAsync(Appointment appointment)
    {
        _db.Appointments.Update(appointment);
        await _db.SaveChangesAsync();
        return appointment;
    }
    public async Task DeleteAsync(int id)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment != null)
        {
            _db.Appointments.Remove(appointment);
            await _db.SaveChangesAsync();
        }
    }
    public async Task<bool> HasConflictAsync(int consultationHourId, DateTime date, TimeSpan time)
        => await _db.Appointments.AnyAsync(a =>
            a.ConsultationHourId == consultationHourId &&
            a.Date.Date == date.Date &&
            a.Time == time &&
            !a.IsCancelled);
    public async Task<bool> HasDoubleBookingAsync(int patientId, DateTime date)
        => await _db.Appointments.AnyAsync(a =>
            a.PatientId == patientId &&
            a.Date.Date == date.Date &&
            !a.IsCancelled);

    // ✅ NEU
    public async Task<List<TimeSpan>> GetBookedSlotsAsync(int consultationHourId, DateOnly date)
        => await _db.Appointments
            .Where(a => a.ConsultationHourId == consultationHourId
                     && DateOnly.FromDateTime(a.Date) == date
                     && !a.IsCancelled)
            .Select(a => a.Time)
            .ToListAsync();
}