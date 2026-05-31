namespace Backend.Model;

public class Doctor
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Title { get; set; }
    public int? UserId { get; set; }
}