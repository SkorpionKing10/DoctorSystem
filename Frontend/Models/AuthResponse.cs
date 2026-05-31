namespace Frontend.Models;

public class AuthResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Role { get; set; } = "";
    public string AuthType { get; set; } = "";
    public string DomainUser { get; set; } = "";
}