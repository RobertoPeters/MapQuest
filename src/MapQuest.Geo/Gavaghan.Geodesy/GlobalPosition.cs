using System.Text;

namespace Gavaghan.Geodesy;

/// <summary>
/// Encapsulates a three dimensional location on a globe (GlobalCoordinates combined with
/// an elevation in meters above a reference ellipsoid).
/// </summary>
[Serializable]
#pragma warning disable S1210 // "Equals" and the comparison operators should be overridden when implementing "IComparable"
public struct GlobalPosition : IComparable<GlobalPosition>
#pragma warning restore S1210 // "Equals" and the comparison operators should be overridden when implementing "IComparable"
{
    /// <summary>Global coordinates.</summary>
    private GlobalCoordinates _coordinates;

    /// <summary>Elevation, in meters, above the surface of the ellipsoid.</summary>
    private double _elevation;

    /// <summary>
    /// Creates a new instance of GlobalPosition.
    /// </summary>
    /// <param name="coords">coordinates on the reference ellipsoid.</param>
    /// <param name="elevation">elevation, in meters, above the reference ellipsoid.</param>
    public GlobalPosition(GlobalCoordinates coords, double elevation)
    {
        _coordinates = coords;
        _elevation = elevation;
    }

    /// <summary>
    /// Creates a new instance of GlobalPosition for a position on the surface of
    /// the reference ellipsoid.
    /// </summary>
    /// <param name="coords"></param>
    public GlobalPosition(GlobalCoordinates coords)
      : this(coords, 0.0)
    {
    }

    /// <summary>Get/set global coordinates.</summary>
#pragma warning disable S2292 // Trivial properties should be auto-implemented
    public GlobalCoordinates Coordinates
#pragma warning restore S2292 // Trivial properties should be auto-implemented
    {
        get { return _coordinates; }
        set { _coordinates = value; }
    }

    /// <summary>Get/set latitude.</summary>
    public Angle Latitude
    {
        get { return _coordinates.Latitude; }
        set { _coordinates.Latitude = value; }
    }

    /// <summary>Get/set longitude.</summary>
    public Angle Longitude
    {
        get { return _coordinates.Longitude; }
        set { _coordinates.Longitude = value; }
    }

    /// <summary>
    /// Get/set elevation, in meters, above the surface of the reference ellipsoid.
    /// </summary>
#pragma warning disable S2292 // Trivial properties should be auto-implemented
    public double Elevation
#pragma warning restore S2292 // Trivial properties should be auto-implemented
    {
        get { return _elevation; }
        set { _elevation = value; }
    }

    /// <summary>
    /// Compare this position to another.  Western longitudes are less than
    /// eastern logitudes.  If longitudes are equal, then southern latitudes are
    /// less than northern latitudes.  If coordinates are equal, lower elevations
    /// are less than higher elevations
    /// </summary>
    /// <param name="other">instance to compare to</param>
    /// <returns>-1, 0, or +1 as per IComparable contract</returns>
    public int CompareTo(GlobalPosition other)
    {
        int retval = _coordinates.CompareTo(other._coordinates);

        if (retval == 0)
        {
            if (_elevation < other._elevation) retval = -1;
            else if (_elevation > other._elevation) retval = +1;
        }

        return retval;
    }

    /// <summary>
    /// Calculate a hash code.
    /// </summary>
    /// <returns></returns>
#pragma warning disable S2328 // "GetHashCode" should not reference mutable fields
    public override int GetHashCode()
#pragma warning restore S2328 // "GetHashCode" should not reference mutable fields
    {
        int hash = _coordinates.GetHashCode();

        if (_elevation != 0) hash *= (int)_elevation;

        return hash;
    }

    /// <summary>
    /// Compare this position to another object for equality.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    public override bool Equals(object obj)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    {
        if (!(obj is GlobalPosition)) return false;

        GlobalPosition other = (GlobalPosition)obj;

        return (_elevation == other._elevation) && (_coordinates.Equals(other._coordinates));
    }

    /// <summary>
    /// Get position as a string.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append(_coordinates.ToString());
        builder.Append(";elevation=");
        builder.Append(_elevation.ToString());
        builder.Append("m");

        return builder.ToString();
    }
}
