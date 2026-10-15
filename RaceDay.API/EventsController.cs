using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;
using System.Security.Claims;

namespace RaceDay.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EventsController(RaceDayDbContext db) => _db = db;

    private int CurrentUserID =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    /// <summary>Lists all events.</summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _db.Events
            .AsNoTracking()
            .Select(e => new
            {
                e.EventID,
                e.EventName,
                e.EventType,
                e.EventDate,
                e.StartTime,
                e.Location,
                e.Description,
                e.RegistrationDeadline,
                e.OrganiserID
            })
            .ToListAsync();

        return Ok(events);
    }

    /// <summary>Gets event details, categories and route.</summary>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEvent(int id)
    {
        var item = await _db.Events
            .AsNoTracking()
            .Include(e => e.Categories)
            .Include(e => e.Route)
            .Where(e => e.EventID == id)
            .Select(e => new
            {
                e.EventID,
                e.EventName,
                e.EventType,
                e.EventDate,
                e.StartTime,
                e.Location,
                e.Description,
                e.RegistrationDeadline,
                e.OrganiserID,
                Categories = e.Categories,
                Route = e.Route
            })
            .FirstOrDefaultAsync();

        return item == null ? NotFound() : Ok(item);
    }

    /// <summary>Creates an event for the signed-in organiser.</summary>
    [Authorize(Roles = "Organiser")]
    [HttpPost]
    public async Task<IActionResult> CreateEvent(EventRequest request)
    {
        if (!new[] { "Running", "Walking", "Cycling" }
            .Contains(request.EventType, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = "Event type must be Running, Walking or Cycling." });

        if (request.EventDate < DateOnly.FromDateTime(DateTime.Today))
            return BadRequest(new { message = "The event date cannot be in the past." });

        if (request.RegistrationDeadline > request.EventDate)
            return BadRequest(new { message = "The registration deadline cannot be after the event date." });

        var item = new RaceEvent
        {
            OrganiserID = CurrentUserID,
            EventName = request.EventName.Trim(),
            EventType = request.EventType,
            EventDate = request.EventDate,
            StartTime = request.StartTime,
            Location = request.Location.Trim(),
            Description = request.Description.Trim(),
            RegistrationDeadline = request.RegistrationDeadline
        };

        _db.Events.Add(item);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEvent), new { id = item.EventID }, item);
    }

    /// <summary>Updates an event owned by the signed-in organiser.</summary>
    [Authorize(Roles = "Organiser")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEvent(int id, EventRequest request)
    {
        var item = await _db.Events.FindAsync(id);

        if (item == null) return NotFound();
        if (item.OrganiserID != CurrentUserID) return Forbid();

        if (request.EventDate < DateOnly.FromDateTime(DateTime.Today) ||
            request.RegistrationDeadline > request.EventDate)
            return BadRequest(new { message = "Check the event date and registration deadline." });

        if (!new[] { "Running", "Walking", "Cycling" }
            .Contains(request.EventType, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = "Invalid event type." });

        item.EventName = request.EventName.Trim();
        item.EventType = request.EventType;
        item.EventDate = request.EventDate;
        item.StartTime = request.StartTime;
        item.Location = request.Location.Trim();
        item.Description = request.Description.Trim();
        item.RegistrationDeadline = request.RegistrationDeadline;

        await _db.SaveChangesAsync();

        return Ok(item);
    }

    /// <summary>Deletes an event owned by the signed-in organiser.</summary>
    [Authorize(Roles = "Organiser")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var item = await _db.Events.FindAsync(id);

        if (item == null) return NotFound();
        if (item.OrganiserID != CurrentUserID) return Forbid();

        var hasEnrolments = await _db.Enrolments.AnyAsync(e => e.EventID == id);
        if (hasEnrolments)
            return Conflict(new { message = "Events with enrolments cannot be deleted." });

        _db.Events.Remove(item);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}