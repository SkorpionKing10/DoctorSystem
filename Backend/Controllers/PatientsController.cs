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
        Console.WriteLine("PatientsController.Get: Called");

        var patients = await _patientService.GetAllAsync();

        Console.WriteLine($"PatientsController.Get: Returned {patients?.Count ?? 0} patients");

        return Ok(patients);
    }

    [Authorize(Policy = "DoctorOderAdmin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);

        if (patient == null)
            return NotFound();

        return Ok(patient);
    }

    [Authorize(Policy = "NurAdmin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Patient patient)
    {
        var created = await _patientService.CreateAsync(patient);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created);
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
        Console.WriteLine($"PatientsController.GetByUsername: Called with username={username}");

        var userIdClaim = User.FindFirst("UserId");

        if (userIdClaim == null)
        {
            Console.WriteLine("PatientsController.GetByUsername: UserId Claim fehlt");
            return Unauthorized();
        }

        var patient = await _patientService.GetByUserIdAsync(
            int.Parse(userIdClaim.Value));

        if (patient == null)
        {
            Console.WriteLine("PatientsController.GetByUsername: Kein Patient gefunden");
            return NotFound(new
            {
                message = "Kein Patient verknüpft."
            });
        }

        Console.WriteLine($"PatientsController.GetByUsername: Found patient {patient.Id}");

        return Ok(new
        {
            patient.Id,
            patient.FirstName,
            patient.LastName
        });
    }
}