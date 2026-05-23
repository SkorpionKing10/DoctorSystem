using Backend.Model;
using Microsoft.EntityFrameworkCore;

namespace Backend.Auth;

public class UserRepository
{
    private readonly DoctorDbContext _db;

    public UserRepository(DoctorDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByDomainUsername(string domainUsername)
    {
        // "PRAXIS\dr.huber" → "dr.huber"
        var username = domainUsername.Contains('\\')
            ? domainUsername.Split('\\')[1].ToLower()
            : domainUsername.ToLower();

        return await _db.Users
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
    }
}