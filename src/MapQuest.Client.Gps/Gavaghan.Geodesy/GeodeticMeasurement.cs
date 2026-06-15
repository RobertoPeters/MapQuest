using System.Text;

namespace Gavaghan.Geodesy;

/// <summary>
/// This is the outcome of a three dimensional geodetic calculation.  It represents
/// the path a between two GlobalPositions for a specified reference ellipsoid.
/// </summary>
[Serializable]
public struct GeodeticMeasurement
{
    /// <summary>The average geodetic curve.</summary>
    private readonly GeodeticCurve _curve;

    /// <summary>The elevation change, in meters, going from the starting to the ending point.</summary>
    private readonly double _elevationChange;

    /// <summary>The distance travelled, in meters, going from one point to the next.</summary>
    private readonly double _p2p;

    /// <summary>
    /// Creates a new instance of GeodeticMeasurement.
    /// </summary>
    /// <param name="averageCurve">the geodetic curve as measured at the average elevation between two points</param>
    /// <param name="elevationChange">the change in elevation, in meters, going from the starting point to the ending point</param>
    public GeodeticMeasurement(GeodeticCurve averageCurve, double elevationChange)
    {
        double ellDist = averageCurve.EllipsoidalDistance;

        _curve = averageCurve;
        _elevationChange = elevationChange;
        _p2p = Math.Sqrt(ellDist * ellDist + _elevationChange * _elevationChange);
    }

    /// <summary>
    /// Get the average geodetic curve.  This is the geodetic curve as measured
    /// at the average elevation between two points.
    /// </summary>
    public GeodeticCurve AverageCurve
    {
        get { return _curve; }
    }

    /// <summary>
    /// Get the ellipsoidal distance (in meters).  This is the length of the average geodetic
    /// curve.  For actual point-to-point distance, use PointToPointDistance property.
    /// </summary>
    public double EllipsoidalDistance
    {
        get { return _curve.EllipsoidalDistance; }
    }

    /// <summary>
    /// Get the azimuth.  This is angle from north from start to end.
    /// </summary>
    public Angle Azimuth
    {
        get { return _curve.Azimuth; }
    }

    /// <summary>
    /// Get the reverse azimuth.  This is angle from north from end to start.
    /// </summary>
    public Angle ReverseAzimuth
    {
        get { return _curve.ReverseAzimuth; }
    }

    /// <summary>
    /// Get the elevation change, in meters, going from the starting to the ending point.
    /// </summary>
    public double ElevationChange
    {
        get { return _elevationChange; }
    }

    /// <summary>
    /// Get the distance travelled, in meters, going from one point to the next.
    /// </summary>
    public double PointToPointDistance
    {
        get { return _p2p; }
    }

    /// <summary>
    /// Get the GeodeticMeasurement as a string
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append(_curve.ToString());
        builder.Append(";elev12=");
        builder.Append(_elevationChange);
        builder.Append(";p2p=");
        builder.Append(_p2p);

        return builder.ToString();
    }
}
