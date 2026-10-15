namespace RaceDay.Models;

public class User
{
    public int UserID { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string Role { get; set; } = "Participant";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RaceEvent> OrganisedEvents { get; set; } = new List<RaceEvent>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}