namespace RaceDay.Models;

public class Enrolment
{
    public int EnrolmentID { get; set; }
    public int ParticipantID { get; set; }
    public int EventID { get; set; }
    public int CategoryID { get; set; }
    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
    public int? BibNumber { get; set; }
    public string Status { get; set; } = "Confirmed";

    public User? Participant { get; set; }
    public RaceEvent? Event { get; set; }
    public Category? Category { get; set; }
    public Result? Result { get; set; }
}