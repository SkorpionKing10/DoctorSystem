using Frontend.Models;

namespace Frontend.Services;

public interface IAppointmentService
{
    Task<List<AppointmentDto>> GetAllAppointmentsAsync();
    Task<List<AppointmentDto>> GetPatientAppointmentsAsync(int patientId);
    Task<List<AppointmentDto>> GetMyAppointmentsAsync();
    Task<List<string>> GetFreeSlotsAsync(int consultationHourId, DateTime date);
    Task<AppointmentDto?> CreateAppointmentAsync(AppointmentCreateDto dto);
    Task<bool> CancelAppointmentAsync(int id);
    Task<bool> UpdateAppointmentAsync(int id, AppointmentDto appointment);
    Task<bool> DeleteAppointmentAsync(int id);
    Task<AppointmentDto?> BookNextAvailableAsync(int patientId, int consultationHourId);
}