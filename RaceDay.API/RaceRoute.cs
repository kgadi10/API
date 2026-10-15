namespace RaceDay.Models;

public class RaceRoute
{
    public int RouteID { get; set; }
    public int EventID { get; set; }
    public string RouteName { get; set; } = "";
    public decimal DistanceKM { get; set; }
    public string StartLocation { get; set; } = "";
    public string EndLocation { get; set; } = "";
    public string RouteDescription { get; set; } = "";

    public RaceEvent? Event { get; set; }
}