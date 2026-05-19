using Backend.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly DoctorDbContext _db;

    public AuthController(DoctorDbContext db)
    {
        _db = db;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == req.Username
                                   && u.PasswordHash == req.Password
                                   && u.IsActive);

        if (user == null) return Unauthorized("Falscher Benutzername oder Passwort");

        return Ok(new
        {
            user.Id,
            user.Username,
            Role = (int)user.Role
        });
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed()
    {
        if (await _db.Users.AnyAsync(u => u.Username == "admin.mueller"))
            return BadRequest("User existiert bereits");

        var user = new User
        {
            Username = "admin.mueller",
            PasswordHash = "Admin1234!",
            Role = UserRole.Admin,
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok("Admin erstellt");
    }
}

public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}