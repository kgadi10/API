using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public UsersController(RaceDayDbContext db) => _db = db;

    private int CurrentUserID =>
        int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    /// <summary>Gets the signed-in user's profile.</summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _db.Users.FindAsync(CurrentUserID);

        if (user == null) return NotFound();

        return Ok(new
        {
            user.UserID,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber,
            user.Role,
            user.CreatedAt
        });
    }

    /// <summary>Updates the signed-in user's profile.</summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(ProfileRequest request)
    {
        var user = await _db.Users.FindAsync(CurrentUserID);

        if (user == null) return NotFound();

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = request.PhoneNumber.Trim();

        await _db.SaveChangesAsync();

        return Ok(new { message = "Profile updated successfully." });
    }
}