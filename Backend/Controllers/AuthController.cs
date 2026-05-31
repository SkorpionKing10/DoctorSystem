using Backend.Auth;
using Backend.Model;
using Backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Principal;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public AuthController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var identity = HttpContext.User.Identity as WindowsIdentity;
        var domainUser = identity?.Name ?? User.Identity?.Name ?? "";

        var user = await _userRepository.GetByDomainUsernameAsync(domainUser);

        if (user == null)
            return Unauthorized(new { message = "User nicht in der Datenbank oder inaktiv." });

        return Ok(new
        {
            user.Id,
            user.Username,
            Role = user.Role.ToString(),
            AuthType = identity?.AuthenticationType ?? "Unknown",
            DomainUser = domainUser
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _userRepository.GetByUsernameAsync(req.Username);

        if (user == null || user.PasswordHash != req.Password)
            return Unauthorized("Falscher Benutzername oder Passwort");

        return Ok(new
        {
            user.Id,
            user.Username,
            Role = user.Role.ToString()
        });
    }
}

public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}