using Backend.Model;
using Backend.Repositories;

namespace Backend.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IConsultationHourRepository _consultationHourRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IUserRepository _userRepository;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IConsultationHourRepository consultationHourRepository,
        IPatientRepository patientRepository,
        IUserRepository userRepository)
    {
        _appointmentRepository = appointmentRepository;
        _consultationHourRepository = consultationHourRepository;
        _patientRepository = patientRepository;
        _userRepository = userRepository;
    }

    public async Task<Appointment?> GetByIdAsync(int id)
        => await _appointmentRepository.GetByIdAsync(id);

    public async Task<List<Appointment>> GetAllAsync()
        => await _appointmentRepository.GetAllAsync();

    public async Task<List<Appointment>> GetByPatientIdAsync(int patientId)
        => await _appointmentRepository.GetByPatientIdAsync(patientId);

    public async Task<List<Appointment>> GetByPatientUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username)
            ?? throw new KeyNotFoundException($"User {username} nicht gefunden.");

        var patient = await _patientRepository.GetByUserIdAsync(user.Id)
            ?? throw new KeyNotFoundException($"Patient für User {username} nicht gefunden.");

        return await _appointmentRepository.GetByPatientIdAsync(patient.Id);
    }

    public async Task<Appointment> CreateAsync(AppointmentCreateDto dto)
    {
        var collision = await _appointmentRepository.HasConflictAsync(dto.ConsultationHourId, dto.Date, dto.Time);
        if (collision)
            throw new InvalidOperationException("Dieser Zeitslot ist bereits vergeben.");

        var doubleBooking = await _appointmentRepository.HasDoubleBookingAsync(dto.PatientId, dto.Date);
        if (doubleBooking)
            throw new InvalidOperationException("Patient hat bereits einen Termin an diesem Tag.");

        var appointment = new Appointment
        {
            PatientId = dto.PatientId,
            ConsultationHourId = dto.ConsultationHourId,
            Date = dto.Date,
            Time = dto.Time
        };

        return await _appointmentRepository.CreateAsync(appointment);
    }

    public async Task<Appointment> UpdateAsync(int id, Appointment appointment)
    {
        var existing = await _appointmentRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Appointment mit ID {id} nicht gefunden.");

        existing.Date = appointment.Date;
        existing.Time = appointment.Time;

        return await _appointmentRepository.UpdateAsync(existing);
    }

    public async Task CancelAsync(int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Appointment mit ID {id} nicht gefunden.");

        appointment.IsCancelled = true;
        await _appointmentRepository.UpdateAsync(appointment);
    }

    public async Task DeleteAsync(int id)
        => await _appointmentRepository.DeleteAsync(id);

    public async Task<Appointment?> BookNextAvailableAsync(int patientId, int consultationHourId)
    {
        var ch = await _consultationHourRepository.GetByIdAsync(consultationHourId);
        if (ch == null) return null;

        for (var t = ch.StartTime; t < ch.EndTime; t += TimeSpan.FromMinutes(15))
        {
            var hasConflict = await _appointmentRepository.HasConflictAsync(consultationHourId, DateTime.Today, t);
            if (hasConflict) continue;

            var hasDoubleBooking = await _appointmentRepository.HasDoubleBookingAsync(patientId, DateTime.Today);
            if (hasDoubleBooking) return null;

            var appointment = new Appointment
            {
                PatientId = patientId,
                ConsultationHourId = consultationHourId,
                Date = DateTime.Today,
                Time = t
            };

            return await _appointmentRepository.CreateAsync(appointment);
        }

        return null;
    }
}