using Backend.Model;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [Authorize(Policy = "DoctorOderAdmin")]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var appointments = await _appointmentService.GetAllAsync();
        return Ok(appointments);
    }

    [Authorize(Policy = "NurStaff")]
    [HttpGet("my-appointments")]
    public async Task<IActionResult> GetMyAppointments()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrWhiteSpace(username))
            return Unauthorized();

        try
        {
            var appointments = await _appointmentService.GetByPatientUsernameAsync(username);
            return Ok(appointments);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "DoctorOderStaffOderAdmin")]
    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetByPatient(int patientId)
    {
        var appointments = await _appointmentService.GetByPatientIdAsync(patientId);
        return Ok(appointments);
    }

    [Authorize(Policy = "DoctorOderStaffOderAdmin")]
    [HttpPost("book")]
    public async Task<IActionResult> Book(int patientId, int consultationHourId)
    {
        try
        {
            var result = await _appointmentService.BookNextAvailableAsync(patientId, consultationHourId);
            return result == null ? BadRequest() : Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "DoctorOderStaffOderAdmin")]  // ✅
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AppointmentCreateDto dto)
    {
        try
        {
            var appointment = await _appointmentService.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = appointment.Id }, appointment);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "DoctorOderAdmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Appointment appointment)
    {
        try
        {
            var updated = await _appointmentService.UpdateAsync(id, appointment);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [Authorize(Policy = "DoctorOderStaffOderAdmin")]
    [HttpPost("cancel/{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await _appointmentService.CancelAsync(id);
            return Ok();
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
        await _appointmentService.DeleteAsync(id);
        return Ok();
    }
    
    [Authorize(Policy = "DoctorOderAdmin")]
    [HttpGet("free-slots/{consultationHourId}/{date}")]
    public async Task<IActionResult> GetFreeSlots(int consultationHourId, string date)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest(new { message = "Ungültiges Datum." });

        var slots = await _appointmentService.GetFreeSlotsAsync(consultationHourId, parsedDate);
        return Ok(slots);
    }

}