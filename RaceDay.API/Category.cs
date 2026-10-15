namespace RaceDay.Models;

public class Category
{
    public int CategoryID { get; set; }
    public int EventID { get; set; }
    public string CategoryName { get; set; } = "";
    public decimal DistanceKM { get; set; }
    public decimal EntryFee { get; set; }
    public int MaximumParticipants { get; set; }

    public RaceEvent? Event { get; set; }
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}