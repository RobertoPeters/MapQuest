using Gavaghan.Geodesy;

namespace MapQuest.Geo;

public static class GeoService
{
    public static double? FromLong(long? lng) => lng == null ? null : (double)lng / 1000.0;
    public static long? FromDouble(double? lng) => lng == null ? null : (long)(lng * 1000.0);

    public static double? CalculateDistance(long? lat1, long? lon1, long? lat2, long? lon2) =>
        CalculateDistance(FromLong(lat1), FromLong(lon1), FromLong(lat2), FromLong(lon2));

    public static double? CalculateDistance(double? lat1, double? lon1, double? lat2, double? lon2)
    {
        if (lat1 == null || lon1 == null || lat2 == null || lon2 == null)
        {
            return null;
        }

        var calc = new GeodeticCalculator();
        var result = calc.CalculateGeodeticMeasurement(Ellipsoid.WGS84, new GlobalPosition(new GlobalCoordinates(new Angle(lat1.Value), new Angle(lon1.Value))), new GlobalPosition(new GlobalCoordinates(new Angle(lat2.Value), new Angle(lon2.Value))));
        return result.PointToPointDistance;
    }

}
