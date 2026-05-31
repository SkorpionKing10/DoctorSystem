using Backend.Model;

namespace Backend.Services;

public interface IConsultationHourService
{
    Task<ConsultationHour?> GetByIdAsync(int id);
    Task<List<ConsultationHour>> GetAllAsync();
    Task<List<string>> GetFreeSlotsAsync(int consultationHourId, DateTime date);
    Task<ConsultationHour> CreateAsync(ConsultationHour consultationHour);
    Task<ConsultationHour> UpdateAsync(int id, ConsultationHour consultationHour);
    Task DeleteAsync(int id);
}