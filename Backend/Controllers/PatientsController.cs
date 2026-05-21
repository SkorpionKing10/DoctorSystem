using Backend.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
    private readonly DoctorDbContext _db;

    public PatientsController(DoctorDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(await _db.Patients.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _db.Patients.FindAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(Patient p)
    {
        _db.Patients.Add(p);
        await _db.SaveChangesAsync();
        return Ok(p);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Patient updated)
    {
        var p = await _db.Patients.FindAsync(id);
        if (p == null) return NotFound();

        p.FirstName = updated.FirstName;
        p.LastName = updated.LastName;
        p.BirthDate = updated.BirthDate;
        p.SocialSecurityNumber = updated.SocialSecurityNumber;

        await _db.SaveChangesAsync();
        return Ok(p);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Patients.FindAsync(id);
        if (p == null) return NotFound();

        _db.Patients.Remove(p);
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("by-username/{username}")]
    public async Task<IActionResult> GetByUsername(string username)
    {
        // User anhand Username suchen
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null) return NotFound("Kein User gefunden.");

        // Patient mit dieser UserId finden
        var patient = await _db.Patients
            .FirstOrDefaultAsync(p => p.UserId == user.Id);

        if (patient == null) return NotFound("Kein Patient verknüpft.");

        return Ok(new { patient.Id, patient.FirstName, patient.LastName });
    }
}