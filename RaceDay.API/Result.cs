namespace RaceDay.Models;

public class Result
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public int? FinishPosition { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public string ResultStatus { get; set; } = "Finished";
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public Enrolment? Enrolment { get; set; }
}