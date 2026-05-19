using Backend.Model;
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

    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(await _db.Doctors.ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(Doctor d)
    {
        _db.Doctors.Add(d);
        await _db.SaveChangesAsync();
        return Ok(d);
    }

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