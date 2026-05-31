using Backend.Model;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [Authorize(Policy = "DoctorOderAdmin")]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var patients = await _patientService.GetAllAsync();
        return Ok(patients);
    }

    [Authorize(Policy = "DoctorOderAdmin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);
        if (patient == null) return NotFound();
        return Ok(patient);
    }

    [Authorize(Policy = "NurAdmin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Patient patient)
    {
        var created = await _patientService.CreateAsync(patient);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Policy = "NurAdmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Patient patient)
    {
        try
        {
            var updated = await _patientService.UpdateAsync(id, patient);
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
        await _patientService.DeleteAsync(id);
        return Ok();
    }

    [Authorize]
    [HttpGet("by-username/{username}")]
    public async Task<IActionResult> GetByUsername(string username)
    {
        var patient = await _patientService.GetByUserIdAsync(User.FindFirst("UserId") != null ? int.Parse(User.FindFirst("UserId")!.Value) : 0);
        if (patient == null) return NotFound("Kein Patient verknüpft.");
        return Ok(new { patient.Id, patient.FirstName, patient.LastName });
    }
}