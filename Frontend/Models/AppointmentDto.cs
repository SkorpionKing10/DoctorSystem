namespace Frontend.Models;

public class AppointmentDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int ConsultationHourId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public bool IsCancelled { get; set; }

    public string? PatientName { get; set; }
}