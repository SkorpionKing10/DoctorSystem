using Backend.Model;
using Backend.Repositories;

namespace Backend.Services;

public class ConsultationHourService : IConsultationHourService
{
    private readonly IConsultationHourRepository _consultationHourRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public ConsultationHourService(IConsultationHourRepository consultationHourRepository, IAppointmentRepository appointmentRepository)
    {
        _consultationHourRepository = consultationHourRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<ConsultationHour?> GetByIdAsync(int id)
        => await _consultationHourRepository.GetByIdAsync(id);

    public async Task<List<ConsultationHour>> GetAllAsync()
        => await _consultationHourRepository.GetAllAsync();

    public async Task<List<string>> GetFreeSlotsAsync(int consultationHourId, DateTime date)
    {
        var ch = await _consultationHourRepository.GetByIdAsync(consultationHourId)
            ?? throw new KeyNotFoundException($"ConsultationHour mit ID {consultationHourId} nicht gefunden.");

        var booked = await _appointmentRepository.GetByConsultationHourAsync(consultationHourId, date);
        var bookedTimes = booked.Select(a => a.Time).ToList();

        var freeSlots = new List<string>();
        for (var t = ch.StartTime; t < ch.EndTime; t += TimeSpan.FromMinutes(15))
        {
            if (!bookedTimes.Contains(t))
                freeSlots.Add(t.ToString(@"hh\:mm"));
        }

        return freeSlots;
    }

    public async Task<ConsultationHour> CreateAsync(ConsultationHour consultationHour)
        => await _consultationHourRepository.CreateAsync(consultationHour);

    public async Task<ConsultationHour> UpdateAsync(int id, ConsultationHour consultationHour)
    {
        var existing = await _consultationHourRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"ConsultationHour mit ID {id} nicht gefunden.");

        existing.Name = consultationHour.Name;
        existing.StartTime = consultationHour.StartTime;
        existing.EndTime = consultationHour.EndTime;
        existing.DoctorId = consultationHour.DoctorId;
        existing.SpecialtyId = consultationHour.SpecialtyId;

        return await _consultationHourRepository.UpdateAsync(existing);
    }

    public async Task DeleteAsync(int id)
        => await _consultationHourRepository.DeleteAsync(id);
}