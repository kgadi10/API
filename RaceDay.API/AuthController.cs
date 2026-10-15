using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;
using System.Security.Claims;

namespace RaceDay.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IPasswordHasher<User> _hasher;

    public AuthController(RaceDayDbContext db, IPasswordHasher<User> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    /// <summary>Registers an account with a hashed password.</summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email))
            return Conflict(new { message = "An account with this email already exists." });

        var role = request.Role.Trim();

        if (role != "Participant" && role != "Organiser")
            return BadRequest(new { message = "Role must be Participant or Organiser." });

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PhoneNumber = request.PhoneNumber.Trim(),
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Created("", new
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

    /// <summary>Signs in and establishes a session cookie.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return Unauthorized(new { message = "Invalid email or password." });

        var verification = _hasher.VerifyHashedPassword(
            user, user.PasswordHash, request.Password);

        if (verification == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Invalid email or password." });

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return Ok(new
        {
            message = "Login successful.",
            user.UserID,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role
        });
    }

    /// <summary>Ends the current session.</summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return Ok(new { message = "Logged out successfully." });
    }

    /// <summary>Returns a forbidden response for access-denied testing.</summary>
    [Authorize]
    [HttpGet("denied")]
    public IActionResult Denied() =>
        StatusCode(403, new { message = "You do not have permission to access this resource." });
}