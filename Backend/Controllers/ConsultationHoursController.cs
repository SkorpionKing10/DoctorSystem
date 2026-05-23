using Backend.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/consultation-hours")]
public class ConsultationHoursController : ControllerBase
{
    private readonly DoctorDbContext _db;

    public ConsultationHoursController(DoctorDbContext db)
    {
        _db = db;
    }

    // Alle dürfen Sprechstunden sehen
    [Authorize(Policy = "DoctorOderStaff")]
    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(await _db.ConsultationHours.ToListAsync());

    // Nur Admin darf Sprechstunden verwalten
    [Authorize(Policy = "NurAdmin")]
    [HttpPost]
    public async Task<IActionResult> Create(ConsultationHour c)
    {
        _db.ConsultationHours.Add(c);
        await _db.SaveChangesAsync();
        return Ok(c);
    }

    [Authorize(Policy = "NurAdmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ConsultationHour updated)
    {
        var c = await _db.ConsultationHours.FindAsync(id);
        if (c == null) return NotFound();

        c.Name = updated.Name;
        c.StartTime = updated.StartTime;
        c.EndTime = updated.EndTime;
        c.DoctorId = updated.DoctorId;
        c.SpecialtyId = updated.SpecialtyId;

        await _db.SaveChangesAsync();
        return Ok(c);
    }

    [Authorize(Policy = "NurAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.ConsultationHours.FindAsync(id);
        if (c == null) return NotFound();

        _db.ConsultationHours.Remove(c);
        await _db.SaveChangesAsync();
        return Ok();
    }
}