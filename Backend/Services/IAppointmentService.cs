using Backend.Model;

namespace Backend.Services;

public interface IAppointmentService
{
    Task<Appointment?> GetByIdAsync(int id);
    Task<List<Appointment>> GetAllAsync();
    Task<List<Appointment>> GetByPatientIdAsync(int patientId);
    Task<List<Appointment>> GetByPatientUsernameAsync(string username);
    Task<Appointment> CreateAsync(AppointmentCreateDto dto);
    Task<Appointment> UpdateAsync(int id, Appointment appointment);
    Task CancelAsync(int id);
    Task DeleteAsync(int id);
    Task<Appointment?> BookNextAvailableAsync(int patientId, int consultationHourId);
    Task<List<string>> GetFreeSlotsAsync(int consultationHourId, DateOnly date);
}