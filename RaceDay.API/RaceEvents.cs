namespace RaceDay.Models;

public class RaceEvent
{
    public int EventID { get; set; }
    public int OrganiserID { get; set; }
    public string EventName { get; set; } = "";
    public string EventType { get; set; } = "Running";
    public DateOnly EventDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public string Location { get; set; } = "";
    public string Description { get; set; } = "";
    public DateOnly RegistrationDeadline { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? Organiser { get; set; }
    public RaceRoute? Route { get; set; }
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}