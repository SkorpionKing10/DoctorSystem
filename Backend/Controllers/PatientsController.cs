using Backend.Model;
using Microsoft.AspNetCore.Authorization;
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

    // Doctor + Staff dürfen Patienten sehen
    [Authorize(Policy = "DoctorOderStaff")]
    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(await _db.Patients.ToListAsync());

    [Authorize(Policy = "DoctorOderStaff")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _db.Patients.FindAsync(id));

    // Staff darf Patienten anlegen
    [Authorize(Policy = "DoctorOderStaff")]
    [HttpPost]
    public async Task<IActionResult> Create(Patient p)
    {
        _db.Patients.Add(p);
        await _db.SaveChangesAsync();
        return Ok(p);
    }

    // Staff darf Patienten bearbeiten
    [Authorize(Policy = "DoctorOderStaff")]
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

    // Nur Admin darf Patienten löschen
    [Authorize(Policy = "NurAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Patients.FindAsync(id);
        if (p == null) return NotFound();

        _db.Patients.Remove(p);
        await _db.SaveChangesAsync();
        return Ok();
    }

    [Authorize(Policy = "DoctorOderStaff")]
    [HttpGet("by-username/{username}")]
    public async Task<IActionResult> GetByUsername(string username)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null) return NotFound("Kein User gefunden.");

        var patient = await _db.Patients
            .FirstOrDefaultAsync(p => p.UserId == user.Id);

        if (patient == null) return NotFound("Kein Patient verknüpft.");

        return Ok(new { patient.Id, patient.FirstName, patient.LastName });
    }
}