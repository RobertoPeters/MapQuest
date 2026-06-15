using System.Text;

namespace Gavaghan.Geodesy;

/// <summary>
/// This is the outcome of a geodetic calculation.  It represents the path and
/// ellipsoidal distance between two GlobalCoordinates for a specified reference
/// ellipsoid.
/// </summary>
[Serializable]
public struct GeodeticCurve
{
    /// <summary>Ellipsoidal distance (in meters).</summary>
    private readonly double _ellipsoidalDistance;

    /// <summary>Azimuth (degrees from north).</summary>
    private readonly Angle _azimuth;

    /// <summary>Reverse azimuth (degrees from north).</summary>
    private readonly Angle _reverseAzimuth;

    /// <summary>
    /// Create a new GeodeticCurve.
    /// </summary>
    /// <param name="ellipsoidalDistance">ellipsoidal distance in meters</param>
    /// <param name="azimuth">azimuth in degrees</param>
    /// <param name="reverseAzimuth">reverse azimuth in degrees</param>
    public GeodeticCurve(double ellipsoidalDistance, Angle azimuth, Angle reverseAzimuth)
    {
        _ellipsoidalDistance = ellipsoidalDistance;
        _azimuth = azimuth;
        _reverseAzimuth = reverseAzimuth;
    }

    /// <summary>Ellipsoidal distance (in meters).</summary>
    public double EllipsoidalDistance
    {
        get { return _ellipsoidalDistance; }
    }

    /// <summary>
    /// Get the azimuth.  This is angle from north from start to end.
    /// </summary>
    public Angle Azimuth
    {
        get { return _azimuth; }
    }

    /// <summary>
    /// Get the reverse azimuth.  This is angle from north from end to start.
    /// </summary>
    public Angle ReverseAzimuth
    {
        get { return _reverseAzimuth; }
    }

    /// <summary>
    /// Get curve as a string.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append("s=");
        builder.Append(_ellipsoidalDistance);
        builder.Append(";a12=");
        builder.Append(_azimuth);
        builder.Append(";a21=");
        builder.Append(_reverseAzimuth);
        builder.Append(";");

        return builder.ToString();
    }
}
