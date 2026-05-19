namespace Backend.Services;

using Backend.Model;
using Microsoft.EntityFrameworkCore;

public class AppointmentService
{
    private readonly DoctorDbContext _db;

    public AppointmentService(DoctorDbContext db)
    {
        _db = db;
    }

    public async Task<Appointment?> BookNextAvailable(int patientId, int consultationHourId)
    {
        var slot = await GetNextFreeSlot(consultationHourId);
        if (slot == null) return null;

        var overlap = await _db.Appointments.AnyAsync(a =>
            a.PatientId == patientId &&
            a.Date == DateTime.Today &&
            !a.IsCancelled);

        if (overlap) return null;

        var appt = new Appointment
        {
            PatientId = patientId,
            ConsultationHourId = consultationHourId,
            Date = DateTime.Today,
            Time = slot.Value
        };

        _db.Appointments.Add(appt);
        await _db.SaveChangesAsync();

        return appt;
    }

    private async Task<TimeSpan?> GetNextFreeSlot(int id)
    {
        var ch = await _db.ConsultationHours.FindAsync(id);
        if (ch == null) return null;

        for (var t = ch.StartTime; t < ch.EndTime; t += TimeSpan.FromMinutes(15))
        {
            var exists = await _db.Appointments.AnyAsync(a =>
                a.ConsultationHourId == id &&
                a.Date == DateTime.Today &&
                a.Time == t &&
                !a.IsCancelled);

            if (!exists) return t;
        }

        return null;
    }

    public async Task Cancel(int id)
    {
        var a = await _db.Appointments.FindAsync(id);
        if (a == null) return;

        a.IsCancelled = true;
        await _db.SaveChangesAsync();
    }
}