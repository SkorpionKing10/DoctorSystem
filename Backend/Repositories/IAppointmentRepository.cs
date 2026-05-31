using Backend.Model;

namespace Backend.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(int id);
    Task<List<Appointment>> GetAllAsync();
    Task<List<Appointment>> GetByPatientIdAsync(int patientId);
    Task<List<Appointment>> GetByConsultationHourAsync(int consultationHourId, DateTime date);
    Task<List<Appointment>> GetPatientAppointmentsByDateAsync(int patientId, DateTime date);
    Task<Appointment> CreateAsync(Appointment appointment);
    Task<Appointment> UpdateAsync(Appointment appointment);
    Task DeleteAsync(int id);
    Task<bool> HasConflictAsync(int consultationHourId, DateTime date, TimeSpan time);
    Task<bool> HasDoubleBookingAsync(int patientId, DateTime date);
}