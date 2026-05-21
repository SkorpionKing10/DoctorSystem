using Backend.Model;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly AppointmentService _service;
    private readonly DoctorDbContext _db;

    public AppointmentsController(AppointmentService service, DoctorDbContext db)
    {
        _service = service;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(await _db.Appointments.ToListAsync());

    [HttpPost("book")]
    public async Task<IActionResult> Book(int patientId, int consultationHourId)
    {
        var result = await _service.BookNextAvailable(patientId, consultationHourId);
        return result == null ? BadRequest() : Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Appointment updated)
    {
        var a = await _db.Appointments.FindAsync(id);
        if (a == null) return NotFound();

        a.Date = updated.Date;
        a.Time = updated.Time;

        await _db.SaveChangesAsync();
        return Ok(a);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var a = await _db.Appointments.FindAsync(id);
        if (a == null) return NotFound();

        _db.Appointments.Remove(a);
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("cancel/{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        await _service.Cancel(id);
        return Ok();
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AppointmentCreateDto dto)
    {
        // Collision Detection
        var collision = await _db.Appointments.AnyAsync(a =>
            a.ConsultationHourId == dto.ConsultationHourId &&
            a.Date.Date == dto.Date.Date &&
            a.Time == dto.Time &&
            !a.IsCancelled);

        if (collision)
            return Conflict(new { message = "Dieser Zeitslot ist bereits vergeben." });

        // Patient darf nicht 2 Termine am selben Tag haben
        var doubleBooking = await _db.Appointments.AnyAsync(a =>
            a.PatientId == dto.PatientId &&
            a.Date.Date == dto.Date.Date &&
            !a.IsCancelled);

        if (doubleBooking)
            return Conflict(new { message = "Patient hat bereits einen Termin an diesem Tag." });

        var appt = new Appointment
        {
            PatientId = dto.PatientId,
            ConsultationHourId = dto.ConsultationHourId,
            Date = dto.Date,
            Time = dto.Time
        };

        _db.Appointments.Add(appt);
        await _db.SaveChangesAsync();
        return Ok(appt);
    }

    [HttpGet("free-slots/{consultationHourId}/{date}")]
    public async Task<IActionResult> GetFreeSlots(int consultationHourId, DateTime date)
    {
        var ch = await _db.ConsultationHours.FindAsync(consultationHourId);
        if (ch == null) return NotFound();

        var booked = await _db.Appointments
            .Where(a => a.ConsultationHourId == consultationHourId
                     && a.Date.Date == date.Date
                     && !a.IsCancelled)
            .Select(a => a.Time)
            .ToListAsync();

        var freeSlots = new List<string>();
        for (var t = ch.StartTime; t < ch.EndTime; t += TimeSpan.FromMinutes(15))
        {
            if (!booked.Contains(t))
                freeSlots.Add(t.ToString(@"hh\:mm"));
        }

        return Ok(freeSlots);
    }

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetByPatient(int patientId)
    {
        var appointments = await _db.Appointments
            .Where(a => a.PatientId == patientId && !a.IsCancelled)
            .OrderBy(a => a.Date)
            .ThenBy(a => a.Time)
            .Select(a => new
            {
                a.Id,
                a.PatientId,
                a.ConsultationHourId,
                a.Date,
                a.Time,
                a.IsCancelled,
                ConsultationHourName = _db.ConsultationHours
                    .Where(c => c.Id == a.ConsultationHourId)
                    .Select(c => c.Name)
                    .FirstOrDefault(),
                DoctorName = _db.ConsultationHours
                    .Where(c => c.Id == a.ConsultationHourId)
                    .Join(_db.Doctors, c => c.DoctorId, d => d.Id,
                          (c, d) => d.Title + " " + d.FirstName + " " + d.LastName)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(appointments);
    }
}