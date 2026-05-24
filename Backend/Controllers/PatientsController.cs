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

    // Nur Doctor + Admin dürfen alle Patienten sehen
    [Authorize(Policy = "DoctorOderAdmin")]
    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(await _db.Patients.ToListAsync());

    [Authorize(Policy = "DoctorOderAdmin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _db.Patients.FindAsync(id));

    // Admin darf Patienten anlegen
    [Authorize(Policy = "NurAdmin")]
    [HttpPost]
    public async Task<IActionResult> Create(Patient p)
    {
        _db.Patients.Add(p);
        await _db.SaveChangesAsync();
        return Ok(p);
    }

    // Admin darf Patienten bearbeiten
    [Authorize(Policy = "NurAdmin")]
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

    // Jeder darf seinen eigenen Patienten-Datensatz sehen
    [Authorize]
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