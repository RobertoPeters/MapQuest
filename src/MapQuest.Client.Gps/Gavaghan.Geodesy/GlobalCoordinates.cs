using System.Text;

namespace Gavaghan.Geodesy;

/// <summary>
/// Encapsulation of latitude and longitude coordinates on a globe.  Negative
/// latitude is southern hemisphere.  Negative longitude is western hemisphere.
/// 
/// Any angle may be specified for longtiude and latitude, but all angles will
/// be canonicalized such that:
/// 
///      -90 <= latitude <= +90
///     -180 < longitude <= +180
/// </summary>
[Serializable]
#pragma warning disable S1210 // "Equals" and the comparison operators should be overridden when implementing "IComparable"
public struct GlobalCoordinates : IComparable<GlobalCoordinates>
#pragma warning restore S1210 // "Equals" and the comparison operators should be overridden when implementing "IComparable"
{
    /// <summary>Latitude.  Negative latitude is southern hemisphere.</summary>
    private Angle _latitude;

    /// <summary>Longitude.  Negative longitude is western hemisphere.</summary>
    private Angle _longitude;

    /// <summary>
    /// Canonicalize the current latitude and longitude values such that:
    /// 
    ///      -90 <= latitude <= +90
    ///     -180 < longitude <= +180
    /// </summary>
    private void Canonicalize()
    {
        double latitude = _latitude.Degrees;
        double longitude = _longitude.Degrees;

        latitude = (latitude + 180) % 360;
        if (latitude < 0) latitude += 360;
        latitude -= 180;

        if (latitude > 90)
        {
            latitude = 180 - latitude;
            longitude += 180;
        }
        else if (latitude < -90)
        {
            latitude = -180 - latitude;
            longitude += 180;
        }

        longitude = ((longitude + 180) % 360);
        if (longitude <= 0) longitude += 360;
        longitude -= 180;

        _latitude = new Angle(latitude);
        _longitude = new Angle(longitude);
    }

    /// <summary>
    /// Construct a new GlobalCoordinates.  Angles will be canonicalized.
    /// </summary>
    /// <param name="latitude">latitude</param>
    /// <param name="longitude">longitude</param>
    public GlobalCoordinates(Angle latitude, Angle longitude)
    {
        _latitude = latitude;
        _longitude = longitude;
        Canonicalize();
    }

    /// <summary>
    /// Get/set latitude.  The latitude value will be canonicalized (which might
    /// result in a change to the longitude). Negative latitude is southern hemisphere.
    /// </summary>
    public Angle Latitude
    {
        get { return _latitude; }
        set
        {
            _latitude = value;
            Canonicalize();
        }
    }

    /// <summary>
    /// Get/set longitude.  The longitude value will be canonicalized. Negative
    /// longitude is western hemisphere.
    /// </summary>
    public Angle Longitude
    {
        get { return _longitude; }
        set
        {
            _longitude = value;
            Canonicalize();
        }
    }

    /// <summary>
    /// Compare these coordinates to another set of coordiates.  Western
    /// longitudes are less than eastern logitudes.  If longitudes are equal,
    /// then southern latitudes are less than northern latitudes.
    /// </summary>
    /// <param name="other">instance to compare to</param>
    /// <returns>-1, 0, or +1 as per IComparable contract</returns>
    public int CompareTo(GlobalCoordinates other)
    {
        int retval;

        if (_longitude < other._longitude) retval = -1;
        else if (_longitude > other._longitude) retval = +1;
        else if (_latitude < other._latitude) retval = -1;
        else if (_latitude > other._latitude) retval = +1;
        else retval = 0;

        return retval;
    }

    /// <summary>
    /// Get a hash code for these coordinates.
    /// </summary>
    /// <returns></returns>
#pragma warning disable S2328 // "GetHashCode" should not reference mutable fields
    public override int GetHashCode()
#pragma warning restore S2328 // "GetHashCode" should not reference mutable fields
    {
        return (_longitude.GetHashCode() * (_latitude.GetHashCode() + 1021)) * 1000033;
    }

    /// <summary>
    /// Compare these coordinates to another object for equality.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    public override bool Equals(object obj)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    {
        if (!(obj is GlobalCoordinates)) return false;

        GlobalCoordinates other = (GlobalCoordinates)obj;

        return (_longitude == other._longitude) && (_latitude == other._latitude);
    }

    /// <summary>
    /// Get coordinates as a string.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append(_latitude.Abs().ToString());
        builder.Append((_latitude >= Angle.Zero) ? 'N' : 'S');
        builder.Append(';');
        builder.Append(_longitude.Abs().ToString());
        builder.Append((_longitude >= Angle.Zero) ? 'E' : 'W');
        builder.Append(';');

        return builder.ToString();
    }
}
