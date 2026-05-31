namespace Backend.Model;

public class Patient
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string SocialSecurityNumber { get; set; } = "";
    public DateTime BirthDate { get; set; }
    public int? UserId { get; set; }  // NEU
}