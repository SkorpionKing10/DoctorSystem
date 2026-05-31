namespace Backend.Model;

public class ConsultationHour
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int DoctorId { get; set; }
    public int SpecialtyId { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsActive { get; set; } = true;
}