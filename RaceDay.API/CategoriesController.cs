using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;
using System.Security.Claims;

namespace RaceDay.Controllers;

[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db) => _db = db;

    private int CurrentUserID =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    /// <summary>Lists categories for an event.</summary>
    [AllowAnonymous]
    [HttpGet("api/events/{eventId:int}/categories")]
    public async Task<IActionResult> GetCategories(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventID == eventId))
            return NotFound(new { message = "Event not found." });

        return Ok(await _db.Categories
            .AsNoTracking()
            .Where(c => c.EventID == eventId)
            .ToListAsync());
    }

    /// <summary>Adds a category to an event owned by the organiser.</summary>
    [Authorize(Roles = "Organiser")]
    [HttpPost("api/events/{eventId:int}/categories")]
    public async Task<IActionResult> CreateCategory(int eventId, CategoryRequest request)
    {
        var item = await _db.Events.FindAsync(eventId);

        if (item == null) return NotFound(new { message = "Event not found." });
        if (item.OrganiserID != CurrentUserID) return Forbid();

        var category = new Category
        {
            EventID = eventId,
            CategoryName = request.CategoryName.Trim(),
            DistanceKM = request.DistanceKM,
            EntryFee = request.EntryFee,
            MaximumParticipants = request.MaximumParticipants
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return Created($"/api/events/{eventId}/categories/{category.CategoryID}", category);
    }

    /// <summary>Updates a category owned by the organiser.</summary>
    [Authorize(Roles = "Organiser")]
    [HttpPut("api/categories/{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, CategoryRequest request)
    {
        var category = await _db.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryID == id);

        if (category == null) return NotFound();
        if (category.Event!.OrganiserID != CurrentUserID) return Forbid();

        category.CategoryName = request.CategoryName.Trim();
        category.DistanceKM = request.DistanceKM;
        category.EntryFee = request.EntryFee;
        category.MaximumParticipants = request.MaximumParticipants;

        await _db.SaveChangesAsync();
        return Ok(category);
    }

    /// <summary>Deletes a category that has no enrolments.</summary>
    [Authorize(Roles = "Organiser")]
    [HttpDelete("api/categories/{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _db.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryID == id);

        if (category == null) return NotFound();
        if (category.Event!.OrganiserID != CurrentUserID) return Forbid();

        if (await _db.Enrolments.AnyAsync(e => e.CategoryID == id))
            return Conflict(new { message = "This category has enrolments and cannot be deleted." });

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}