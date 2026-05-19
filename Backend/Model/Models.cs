namespace Backend.Model;

public enum UserRole
{
    Admin,
    Doctor,
    Staff
}

public class MedicalSpecialty
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Doctor
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Title { get; set; }
    public int? UserId { get; set; }
}

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

public class Patient
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string SocialSecurityNumber { get; set; } = "";
    public DateTime BirthDate { get; set; }
}

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