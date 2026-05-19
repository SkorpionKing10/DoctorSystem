namespace Backend.Model;

public class AppointmentCreateDto
{
    public int PatientId { get; set; }
    public int ConsultationHourId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
}