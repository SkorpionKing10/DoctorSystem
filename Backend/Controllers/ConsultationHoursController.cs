using Backend.Model;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/consultation-hours")]
public class ConsultationHoursController : ControllerBase
{
    private readonly IConsultationHourService _consultationHourService;

    public ConsultationHoursController(IConsultationHourService consultationHourService)
    {
        _consultationHourService = consultationHourService;
    }

    // ✅ Admin auch erlaubt!
    [Authorize(Policy = "DoctorOderStaffOderAdmin")]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        Console.WriteLine("ConsultationHoursController.Get: Called");
        var hours = await _consultationHourService.GetAllAsync();
        Console.WriteLine($"ConsultationHoursController.Get: Returned {hours?.Count ?? 0} consultation hours");
        return Ok(hours);
    }

    [Authorize(Policy = "DoctorOderStaffOderAdmin")]
    [HttpGet("free-slots/{consultationHourId}/{date}")]
    public async Task<IActionResult> GetFreeSlots(int consultationHourId, DateTime date)
    {
        Console.WriteLine($"GetFreeSlots: consultationHourId={consultationHourId}, date={date}");
        try
        {
            var freeSlots = await _consultationHourService.GetFreeSlotsAsync(consultationHourId, date);
            Console.WriteLine($"GetFreeSlots: Found {freeSlots?.Count ?? 0} free slots");
            return Ok(freeSlots);
        }
        catch (KeyNotFoundException ex)
        {
            Console.WriteLine($"GetFreeSlots Exception: {ex.Message}");
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "NurAdmin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ConsultationHour consultationHour)
    {
        var created = await _consultationHourService.CreateAsync(consultationHour);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [Authorize(Policy = "NurAdmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ConsultationHour consultationHour)
    {
        try
        {
            var updated = await _consultationHourService.UpdateAsync(id, consultationHour);
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
        await _consultationHourService.DeleteAsync(id);
        return Ok();
    }
}