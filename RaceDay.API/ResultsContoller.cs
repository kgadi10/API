using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;
using System.Security.Claims;

namespace RaceDay.Controllers;

[ApiController]
[Route("api/results")]
[Authorize]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public ResultsController(RaceDayDbContext db) => _db = db;

    private int CurrentUserID =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    /// <summary>Gets results for the signed-in participant.</summary>
    [Authorize(Roles = "Participant")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyResults()
    {
        var results = await _db.Results
            .AsNoTracking()
            .Include(r => r.Enrolment)!
                .ThenInclude(e => e!.Event)
            .Where(r => r.Enrolment!.ParticipantID == CurrentUserID)
            .ToListAsync();

        return Ok(results);
    }

    /// <summary>Lists results for an event.</summary>
    [AllowAnonymous]
    [HttpGet("/api/events/{eventId:int}/results")]
    public async Task<IActionResult> GetEventResults(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventID == eventId))
            return NotFound(new { message = "Event not found." });

        var results = await _db.Results
            .AsNoTracking()
            .Include(r => r.Enrolment)!
                .ThenInclude(e => e!.Participant)
            .Where(r => r.Enrolment!.EventID == eventId)
            .Select(r => new
            {
                r.ResultID,
                r.FinishPosition,
                r.FinishTime,
                r.ResultStatus,
                r.RecordedAt,
                Participant = r.Enrolment!.Participant == null ? null : new
                {
                    r.Enrolment.Participant.FirstName,
                    r.Enrolment.Participant.LastName
                }
            })
            .OrderBy(r => r.FinishPosition)
            .ToListAsync();

        return Ok(results);
    }

    /// <summary>Records a result for an event enrolment owned by the organiser.</summary>
    [Authorize(Roles = "Organiser")]
    [HttpPost]
    public async Task<IActionResult> CreateResult(
        [FromQuery] int enrolmentId, ResultRequest request)
    {
        var enrolment = await _db.Enrolments
            .Include(e => e.Event)
            .FirstOrDefaultAsync(e => e.EnrolmentID == enrolmentId);

        if (enrolment == null) return NotFound(new { message = "Enrolment not found." });
        if (enrolment.Event!.OrganiserID != CurrentUserID) return Forbid();

        if (await _db.Results.AnyAsync(r => r.EnrolmentID == enrolmentId))
            return Conflict(new { message = "A result already exists for this enrolment." });

        if (request.FinishPosition < 1)
            return BadRequest(new { message = "Finish position must be positive." });

        var result = new Result
        {
            EnrolmentID = enrolmentId,
            FinishPosition = request.FinishPosition,
            FinishTime = request.FinishTime,
            ResultStatus = request.ResultStatus.Trim(),
            RecordedAt = DateTime.UtcNow
        };

        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        return Created($"/api/results/{result.ResultID}", result);
    }

    /// <summary>Updates a result belonging to the organiser's event.</summary>
    [Authorize(Roles = "Organiser")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateResult(int id, ResultRequest request)
    {
        var result = await _db.Results
            .Include(r => r.Enrolment)!
                .ThenInclude(e => e!.Event)
            .FirstOrDefaultAsync(r => r.ResultID == id);

        if (result == null) return NotFound();
        if (result.Enrolment!.Event!.OrganiserID != CurrentUserID) return Forbid();

        if (request.FinishPosition < 1)
            return BadRequest(new { message = "Finish position must be positive." });

        result.FinishPosition = request.FinishPosition;
        result.FinishTime = request.FinishTime;
        result.ResultStatus = request.ResultStatus.Trim();
        result.RecordedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(result);
    }

    /// <summary>Deletes a result belonging to the organiser's event.</summary>
    [Authorize(Roles = "Organiser")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteResult(int id)
    {
        var result = await _db.Results
            .Include(r => r.Enrolment)!
                .ThenInclude(e => e!.Event)
            .FirstOrDefaultAsync(r => r.ResultID == id);

        if (result == null) return NotFound();
        if (result.Enrolment!.Event!.OrganiserID != CurrentUserID) return Forbid();

        _db.Results.Remove(result);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}