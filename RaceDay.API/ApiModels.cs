using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models;

public class RegisterRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = "";

    [Required, MaxLength(100)]
    public string LastName { get; set; } = "";

    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = "";

    [Required, MinLength(8)]
    public string Password { get; set; } = "";

    [MaxLength(30)]
    public string PhoneNumber { get; set; } = "";

    [Required]
    public string Role { get; set; } = "Participant";
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}

public class ProfileRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = "";

    [Required, MaxLength(100)]
    public string LastName { get; set; } = "";

    [MaxLength(30)]
    public string PhoneNumber { get; set; } = "";
}

public class EventRequest
{
    [Required, MaxLength(150)]
    public string EventName { get; set; } = "";

    [Required]
    public string EventType { get; set; } = "Running";

    public DateOnly EventDate { get; set; }
    public TimeOnly StartTime { get; set; }

    [Required, MaxLength(200)]
    public string Location { get; set; } = "";

    [Required, MaxLength(2000)]
    public string Description { get; set; } = "";

    public DateOnly RegistrationDeadline { get; set; }
}

public class CategoryRequest
{
    [Required, MaxLength(100)]
    public string CategoryName { get; set; } = "";

    [Range(0.1, 500)]
    public decimal DistanceKM { get; set; }

    [Range(0, 100000)]
    public decimal EntryFee { get; set; }

    [Range(1, 100000)]
    public int MaximumParticipants { get; set; }
}

public class RouteRequest
{
    [Required, MaxLength(150)]
    public string RouteName { get; set; } = "";

    [Range(0.1, 500)]
    public decimal DistanceKM { get; set; }

    [Required, MaxLength(200)]
    public string StartLocation { get; set; } = "";

    [Required, MaxLength(200)]
    public string EndLocation { get; set; } = "";

    [MaxLength(2000)]
    public string RouteDescription { get; set; } = "";
}

public class EnrolmentRequest
{
    [Range(1, int.MaxValue)]
    public int EventID { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryID { get; set; }
}

public class ResultRequest
{
    public int? FinishPosition { get; set; }
    public TimeSpan? FinishTime { get; set; }

    [Required]
    public string ResultStatus { get; set; } = "Finished";
}