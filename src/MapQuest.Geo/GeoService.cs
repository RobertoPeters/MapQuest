using Gavaghan.Geodesy;
using System.Globalization;

namespace MapQuest.Geo;

public static class GeoService
{
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

    public static string GetCoordinatesPresentation(double? lat, double? lon)
    {
        if (lat == null || lon == null)
        {
            return "";
        }

        string NS = "N ";
        string EW = "E ";
        if (lat < 0)
        {
            lat = Math.Abs(lat.Value);
            NS = "S ";
        }
        if (lon < 0)
        {
            lon = Math.Abs(lon.Value);
            EW = "W ";
        }

        double minutes1 = 60.0 * (lat.Value - (int)lat.Value);
        double minutes2 = 60.0 * (lon.Value - (int)lon.Value);
        string d1 = (1000.0 * (minutes1 - (int)minutes1)).ToString("000");
        if (d1.Length > 3)
        {
            minutes1 += 1;
            d1 = "000";
        }
        string d2 = (1000.0 * (minutes2 - (int)minutes2)).ToString("000");
        if (d2.Length > 3)
        {
            minutes2 += 1;
            d2 = "000";
        }
        return $"{NS}{(int)lat}° {(int)minutes1}.{d1} {EW}{(int)lon}° {(int)minutes2}.{d2}";
    }

    public static (double? Lat, double? Lon) StringToLocation(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return (null, null);
        }

        try
        {
            double? lat = null;
            double? lon = null;
            string[] parts = s.Split(new char[] { ' ', 'N', 'E', 'S', 'W', '.', '°', ',', '\'', '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 6 || parts.Length == 4)
            {
                if (parts.Length == 6)
                {
                    lat = double.Parse(parts[0]) + ((double.Parse(parts[1]) + (double.Parse(parts[2]) / 1000.0)) / 60.0);
                    lon = double.Parse(parts[3]) + ((double.Parse(parts[4]) + (double.Parse(parts[5]) / 1000.0)) / 60.0);

                    if (s.IndexOf("S", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        lat = -1.0 * lat;
                    }
                    if (s.IndexOf("W", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        lon = -1.0 * lon;
                    }
                }
                else
                {
                    lat = double.Parse($"{parts[0]}.{parts[1]}", CultureInfo.InvariantCulture);
                    lon = double.Parse($"{parts[2]}.{parts[3]}", CultureInfo.InvariantCulture);
                }

            }
            return (lat, lon);
        }
        catch
        {
            return (null, null);
        }
    }
}
