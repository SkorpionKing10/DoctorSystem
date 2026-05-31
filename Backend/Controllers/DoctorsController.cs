using Backend.Model;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/doctors")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [Authorize(Policy = "DoctorOderStaff")]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var doctors = await _doctorService.GetAllAsync();
        return Ok(doctors);
    }

    [Authorize(Policy = "NurAdmin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Doctor doctor)
    {
        var created = await _doctorService.CreateAsync(doctor);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [Authorize(Policy = "NurAdmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Doctor doctor)
    {
        try
        {
            var updated = await _doctorService.UpdateAsync(id, doctor);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [Authorize(Policy = "NurAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _doctorService.DeleteAsync(id);
        return Ok();
    }
}