using Frontend.Models;

namespace Frontend.Services;

public interface IConsultationHourService
{
    Task<List<ConsultationHourDto>> GetAllAsync();
    Task<ConsultationHourDto?> GetByIdAsync(int id);
}