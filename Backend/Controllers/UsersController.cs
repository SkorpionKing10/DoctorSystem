namespace Backend.Controllers;

using Backend.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly DoctorDbContext _db;

    public UsersController(DoctorDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _db.Users.Select(u => new
        {
            u.Id,
            u.Username,
            Role = u.Role.ToString(),
            u.IsActive,
            u.CreatedAt
        }).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Benutzername und Passwort sind erforderlich.");

        if (!Enum.TryParse<UserRole>(dto.Role, ignoreCase: true, out var role))
            return BadRequest("Ungültige Rolle. Erlaubt: Admin, Doctor, Staff");

        var exists = await _db.Users.AnyAsync(u => u.Username == dto.Username);
        if (exists)
            return Conflict("Benutzername bereits vergeben.");

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = dto.Password,  // Hash kommt später
            Role = role,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = user.Id }, new
        {
            user.Id,
            user.Username,
            Role = user.Role.ToString(),
            user.IsActive,
            user.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateUserDto dto)
    {
        var u = await _db.Users.FindAsync(id);
        if (u == null) return NotFound();

        u.Username = dto.Username;
        u.Role = dto.Role;
        u.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();
        return Ok(u);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var u = await _db.Users.FindAsync(id);
        if (u == null) return NotFound();

        _db.Users.Remove(u);
        await _db.SaveChangesAsync();
        return Ok();
    }
}

public class CreateUserDto
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "Staff";
    public bool IsActive { get; set; } = true;
}

public class UpdateUserDto
{
    public string Username { get; set; } = "";
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
}