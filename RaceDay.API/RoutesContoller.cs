using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;
using System.Security.Claims;

namespace RaceDay.Controllers;

[ApiController]
public class RoutesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public RoutesController(RaceDayDbContext db) => _db = db;

    private int CurrentUserID =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    /// <summary>Gets the route for an event.</summary>
    [AllowAnonymous]
    [HttpGet("api/events/{eventId:int}/route")]
    public async Task<IActionResult> GetRoute(int eventId)
    {
        var route = await _db.Routes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.EventID == eventId);

        return route == null ? NotFound() : Ok(route);
    }

    /// <summary>Creates a route for an event owned by the organiser.</summary>
    [Authorize(Roles = "Organiser")]
    [HttpPost("api/events/{eventId:int}/route")]
    public async Task<IActionResult> CreateRoute(int eventId, RouteRequest request)
    {
        var item = await _db.Events.FindAsync(eventId);

        if (item == null) return NotFound(new { message = "Event not found." });
        if (item.OrganiserID != CurrentUserID) return Forbid();

        if (await _db.Routes.AnyAsync(r => r.EventID == eventId))
            return Conflict(new { message = "A route already exists for this event." });

        var route = new RaceRoute
        {
            EventID = eventId,
            RouteName = request.RouteName.Trim(),
            DistanceKM = request.DistanceKM,
            StartLocation = request.StartLocation.Trim(),
            EndLocation = request.EndLocation.Trim(),
            RouteDescription = request.RouteDescription.Trim()
        };

        _db.Routes.Add(route);
        await _db.SaveChangesAsync();

        return Created($"/api/events/{eventId}/route", route);
    }

    /// <summary>Updates a route owned by the organiser.</summary>
    [Authorize(Roles = "Organiser")]
    [HttpPut("api/routes/{id:int}")]
    public async Task<IActionResult> UpdateRoute(int id, RouteRequest request)
    {
        var route = await _db.Routes
            .Include(r => r.Event)
            .FirstOrDefaultAsync(r => r.RouteID == id);

        if (route == null) return NotFound();
        if (route.Event!.OrganiserID != CurrentUserID) return Forbid();

        route.RouteName = request.RouteName.Trim();
        route.DistanceKM = request.DistanceKM;
        route.StartLocation = request.StartLocation.Trim();
        route.EndLocation = request.EndLocation.Trim();
        route.RouteDescription = request.RouteDescription.Trim();

        await _db.SaveChangesAsync();
        return Ok(route);
    }
}