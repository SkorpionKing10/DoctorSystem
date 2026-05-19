namespace Backend.Model;
public class UpdateUserDto
{
    public string Username { get; set; } = "";
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
}