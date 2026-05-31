namespace Frontend.Models;

public class UpdateUserDto
{
    public string Username { get; set; } = "";
    public string Role { get; set; } = "";
    public bool IsActive { get; set; }
}