namespace Backend.Model;

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int ConsultationHourId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}