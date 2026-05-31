using Backend.Model;

namespace Backend.Repositories;

public interface IConsultationHourRepository
{
    Task<ConsultationHour?> GetByIdAsync(int id);
    Task<List<ConsultationHour>> GetAllAsync();
    Task<ConsultationHour> CreateAsync(ConsultationHour consultationHour);
    Task<ConsultationHour> UpdateAsync(ConsultationHour consultationHour);
    Task DeleteAsync(int id);
}