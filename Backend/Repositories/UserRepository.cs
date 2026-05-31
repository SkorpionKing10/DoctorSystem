using Backend.Model;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DoctorDbContext _db;

    public UserRepository(DoctorDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(int id)
        => await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsActive);

    public async Task<User?> GetByUsernameAsync(string username)
        => await _db.Users.FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

    public async Task<User?> GetByDomainUsernameAsync(string domainUsername)
    {
        var username = domainUsername.Contains('\\')
            ? domainUsername.Split('\\')[1].ToLower()
            : domainUsername.ToLower();

        return await _db.Users.FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
    }

    public async Task<List<User>> GetAllAsync()
        => await _db.Users.Where(u => u.IsActive).ToListAsync();

    public async Task<User> CreateAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user != null)
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string username)
        => await _db.Users.AnyAsync(u => u.Username == username);
}