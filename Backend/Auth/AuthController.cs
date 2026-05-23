using Backend.Auth;
using Backend.Model;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Principal;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly DoctorDbContext _db;
    private readonly UserRepository _userRepo;

    public AuthController(DoctorDbContext db, UserRepository userRepo)
    {
        _db = db;
        _userRepo = userRepo;
    }

    // Kerberos: Wer bin ich? → für Blazor Frontend
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var identity = HttpContext.User.Identity as WindowsIdentity;
        var domainUser = identity?.Name ?? User.Identity?.Name ?? "";

        var user = await _userRepo.GetByDomainUsername(domainUser);

        if (user == null)
            return Unauthorized(new { message = "User nicht in der Datenbank oder inaktiv." });

        return Ok(new
        {
            user.Id,
            user.Username,
            Role = user.Role.ToString(),
            AuthType = identity?.AuthenticationType ?? "Unknown", // "Kerberos" oder "NTLM"
            DomainUser = domainUser
        });
    }

    // ALT: Password Login bleibt als Fallback erhalten
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == req.Username
                                   && u.PasswordHash == req.Password
                                   && u.IsActive);

        if (user == null)
            return Unauthorized("Falscher Benutzername oder Passwort");

        return Ok(new
        {
            user.Id,
            user.Username,
            Role = user.Role.ToString()
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