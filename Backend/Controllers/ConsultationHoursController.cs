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

    [Authorize(Policy = "DoctorOderStaff")]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var hours = await _consultationHourService.GetAllAsync();
        return Ok(hours);
    }

    [Authorize(Policy = "DoctorOderStaff")]
    [HttpGet("free-slots/{consultationHourId}/{date}")]
    public async Task<IActionResult> GetFreeSlots(int consultationHourId, DateTime date)
    {
        try
        {
            var freeSlots = await _consultationHourService.GetFreeSlotsAsync(consultationHourId, date);
            return Ok(freeSlots);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
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