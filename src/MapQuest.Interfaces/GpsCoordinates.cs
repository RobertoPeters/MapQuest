namespace MapQuest.Interfaces;

public class GpsCoordinates
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Accuracy { get; set; }
    public double? Altitude { get; set; }
    public double? Heading { get; set; }
    public double? Speed { get; set; }
    public long Timestamp { get; set; }
}
