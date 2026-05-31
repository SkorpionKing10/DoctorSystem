namespace Backend.Model;

public class CreateUserDto
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "Staff";
    public bool IsActive { get; set; } = true;
}