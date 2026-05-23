using Backend.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Principal;

namespace Backend.Auth;

public class KerberosRollenTransformer : IClaimsTransformation
{
    private readonly DoctorDbContext _db;

    public KerberosRollenTransformer(DoctorDbContext db)
    {
        _db = db;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Nur wenn Windows/Kerberos Auth bestätigt
        if (principal.Identity is not WindowsIdentity { IsAuthenticated: true } identity)
            return principal;

        // "PRAXIS\dr.huber" → "dr.huber"
        var username = identity.Name.Contains('\\')
            ? identity.Name.Split('\\')[1].ToLower()
            : identity.Name.ToLower();

        // Rolle aus deiner Users-Tabelle holen
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

        if (user == null)
            return principal; // Nicht in DB oder IsActive=0 → kein Zugriff

        // Rolle als Claim hinzufügen
        var claims = new ClaimsIdentity();
        claims.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));
        claims.AddClaim(new Claim(ClaimTypes.Name, user.Username));
        claims.AddClaim(new Claim("UserId", user.Id.ToString()));
        principal.AddIdentity(claims);

        return principal;
    }
}