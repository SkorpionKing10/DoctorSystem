using Backend.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/doctors")]
public class DoctorsController : ControllerBase
{
    private readonly DoctorDbContext _db;

    public DoctorsController(DoctorDbContext db)
    {
        _db = db;
    }

    // Alle dürfen Ärzte sehen
    [Authorize(Policy = "DoctorOderStaff")]
    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(await _db.Doctors.ToListAsync());

    // Nur Admin darf Ärzte anlegen/bearbeiten/löschen
    [Authorize(Policy = "NurAdmin")]
    [HttpPost]
    public async Task<IActionResult> Create(Doctor d)
    {
        _db.Doctors.Add(d);
        await _db.SaveChangesAsync();
        return Ok(d);
    }

    [Authorize(Policy = "NurAdmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Doctor updated)
    {
        var d = await _db.Doctors.FindAsync(id);
        if (d == null) return NotFound();

        d.FirstName = updated.FirstName;
        d.LastName = updated.LastName;
        d.Title = updated.Title;

        await _db.SaveChangesAsync();
        return Ok(d);
    }

    [Authorize(Policy = "NurAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var d = await _db.Doctors.FindAsync(id);
        if (d == null) return NotFound();

        _db.Doctors.Remove(d);
        await _db.SaveChangesAsync();
        return Ok();
    }
}