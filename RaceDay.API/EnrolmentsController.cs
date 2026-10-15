using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;
using System.Security.Claims;

namespace RaceDay.Controllers;

[ApiController]
[Route("api/enrolments")]
[Authorize]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db) => _db = db;

    private int CurrentUserID =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    /// <summary>Enrols the signed-in participant in an event category.</summary>
    [Authorize(Roles = "Participant")]
    [HttpPost]
    public async Task<IActionResult> CreateEnrolment(EnrolmentRequest request)
    {
        var item = await _db.Events.FindAsync(request.EventID);
        var category = await _db.Categories.FindAsync(request.CategoryID);

        if (item == null || category == null)
            return NotFound(new { message = "Event or category not found." });

        if (category.EventID != item.EventID)
            return BadRequest(new { message = "The category does not belong to this event." });

        if (item.RegistrationDeadline < DateOnly.FromDateTime(DateTime.Today))
            return BadRequest(new { message = "Registration has closed." });

        if (item.EventDate < DateOnly.FromDateTime(DateTime.Today))
            return BadRequest(new { message = "This event has already taken place." });

        var count = await _db.Enrolments.CountAsync(e =>
            e.CategoryID == category.CategoryID && e.Status == "Confirmed");

        if (count >= category.MaximumParticipants)
            return Conflict(new { message = "This category is full." });

        if (await _db.Enrolments.AnyAsync(e =>
            e.ParticipantID == CurrentUserID &&
            e.EventID == request.EventID &&
            e.CategoryID == request.CategoryID))
            return Conflict(new { message = "You are already enrolled in this category." });

        var enrolment = new Enrolment
        {
            ParticipantID = CurrentUserID,
            EventID = request.EventID,
            CategoryID = request.CategoryID,
            EnrolmentDate = DateTime.UtcNow,
            Status = "Confirmed"
        };

        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        return Created($"/api/enrolments/{enrolment.EnrolmentID}", enrolment);
    }

    /// <summary>Lists enrolments belonging to the signed-in participant.</summary>
    [Authorize(Roles = "Participant")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyEnrolments()
    {
        var enrolments = await _db.Enrolments
            .AsNoTracking()
            .Include(e => e.Event)
            .Include(e => e.Category)
            .Where(e => e.ParticipantID == CurrentUserID)
            .ToListAsync();

        return Ok(enrolments);
    }

    /// <summary>Lists enrolments for an event owned by the organiser.</summary>
    [Authorize(Roles = "Organiser")]
    [HttpGet("/api/events/{eventId:int}/enrolments")]
    public async Task<IActionResult> GetEventEnrolments(int eventId)
    {
        var item = await _db.Events.FindAsync(eventId);

        if (item == null) return NotFound();
        if (item.OrganiserID != CurrentUserID) return Forbid();

        var enrolments = await _db.Enrolments
            .AsNoTracking()
            .Include(e => e.Participant)
            .Include(e => e.Category)
            .Where(e => e.EventID == eventId)
            .Select(e => new
            {
                e.EnrolmentID,
                e.EventID,
                e.CategoryID,
                e.EnrolmentDate,
                e.BibNumber,
                e.Status,
                Participant = e.Participant == null ? null : new
                {
                    e.Participant.UserID,
                    e.Participant.FirstName,
                    e.Participant.LastName,
                    e.Participant.Email
                },
                Category = e.Category == null ? null : e.Category.CategoryName
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    /// <summary>Cancels the signed-in participant's enrolment.</summary>
    [Authorize(Roles = "Participant")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> CancelEnrolment(int id)
    {
        var enrolment = await _db.Enrolments
            .FirstOrDefaultAsync(e => e.EnrolmentID == id);

        if (enrolment == null) return NotFound();
        if (enrolment.ParticipantID != CurrentUserID) return Forbid();

        enrolment.Status = "Cancelled";
        await _db.SaveChangesAsync();

        return NoContent();
    }
}