namespace Gavaghan.Geodesy;

/// <summary>
/// Encapsulation of an Angle.  Angles are constructed and serialized in
/// degrees for human convenience, but a conversion to radians is provided
/// for mathematical calculations.
/// 
/// Angle comparisons are performed in absolute terms - no "wrapping" occurs.
/// In other words, 360 degress != 0 degrees.
/// </summary>
[Serializable]
public struct Angle : IComparable<Angle>
{
    /// <summary>Degrees/Radians conversion constant.</summary>
    private const double PiOver180 = Math.PI / 180.0;

    /// <summary>Angle value in degrees.</summary>
    private double _degrees;

    /// <summary>Zero Angle</summary>
    static public readonly Angle Zero = new Angle(0);

    /// <summary>180 degree Angle</summary>
    static public readonly Angle Angle180 = new Angle(180);

    /// <summary>
    /// Construct a new Angle from a degree measurement.
    /// </summary>
    /// <param name="degrees">angle measurement</param>
    public Angle(double degrees)
    {
        _degrees = degrees;
    }

    /// <summary>
    /// Construct a new Angle from degrees and minutes.
    /// </summary>
    /// <param name="degrees">degree portion of angle measurement</param>
    /// <param name="minutes">minutes portion of angle measurement (0 <= minutes < 60)</param>
    public Angle(int degrees, double minutes)
    {
        _degrees = minutes / 60.0;

        _degrees = (degrees < 0) ? (degrees - _degrees) : (degrees + _degrees);
    }

    /// <summary>
    /// Construct a new Angle from degrees, minutes, and seconds.
    /// </summary>
    /// <param name="degrees">degree portion of angle measurement</param>
    /// <param name="minutes">minutes portion of angle measurement (0 <= minutes < 60)</param>
    /// <param name="seconds">seconds portion of angle measurement (0 <= seconds < 60)</param>
    public Angle(int degrees, int minutes, double seconds)
    {
        _degrees = (seconds / 3600.0) + (minutes / 60.0);

        _degrees = (degrees < 0) ? (degrees - _degrees) : (degrees + _degrees);
    }

    /// <summary>
    /// Get/set angle measured in degrees.
    /// </summary>
#pragma warning disable S2292 // Trivial properties should be auto-implemented
    public double Degrees
#pragma warning restore S2292 // Trivial properties should be auto-implemented
    {
        get { return _degrees; }
        set { _degrees = value; }
    }

    /// <summary>
    /// Get/set angle measured in radians.
    /// </summary>
    public double Radians
    {
        get { return _degrees * PiOver180; }
        set { _degrees = value / PiOver180; }
    }

    /// <summary>
    /// Get the absolute value of the angle.
    /// </summary>
    public Angle Abs()
    {
        return new Angle(Math.Abs(_degrees));
    }

    /// <summary>
    /// Compare this angle to another angle.
    /// </summary>
    /// <param name="other">other angle to compare to.</param>
    /// <returns>result according to IComparable contract/></returns>
    public int CompareTo(Angle other)
    {
        return _degrees.CompareTo(other._degrees);
    }

    /// <summary>
    /// Calculate a hash code for the angle.
    /// </summary>
    /// <returns>hash code</returns>
#pragma warning disable S2328 // "GetHashCode" should not reference mutable fields
    public override int GetHashCode()
#pragma warning restore S2328 // "GetHashCode" should not reference mutable fields
    {
        return (int)(_degrees * 1000033);
    }

    /// <summary>
    /// Compare this Angle to another Angle for equality.  Angle comparisons
    /// are performed in absolute terms - no "wrapping" occurs.  In other
    /// words, 360 degress != 0 degrees.
    /// </summary>
    /// <param name="obj">object to compare to</param>
    /// <returns>'true' if angles are equal</returns>
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    public override bool Equals(object obj)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    {
        if (!(obj is Angle)) return false;

        Angle other = (Angle)obj;

        return _degrees == other._degrees;
    }

    /// <summary>
    /// Get coordinates as a string.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return _degrees.ToString();
    }

    #region Operators
    public static Angle operator +(Angle lhs, Angle rhs)
    {
        return new Angle(lhs._degrees + rhs._degrees);
    }

    public static Angle operator -(Angle lhs, Angle rhs)
    {
        return new Angle(lhs._degrees - rhs._degrees);
    }

    public static bool operator >(Angle lhs, Angle rhs)
    {
        return lhs._degrees > rhs._degrees;
    }

    public static bool operator >=(Angle lhs, Angle rhs)
    {
        return lhs._degrees >= rhs._degrees;
    }

    public static bool operator <(Angle lhs, Angle rhs)
    {
        return lhs._degrees < rhs._degrees;
    }

    public static bool operator <=(Angle lhs, Angle rhs)
    {
        return lhs._degrees <= rhs._degrees;
    }

    public static bool operator ==(Angle lhs, Angle rhs)
    {
        return lhs._degrees == rhs._degrees;
    }

    public static bool operator !=(Angle lhs, Angle rhs)
    {
        return lhs._degrees != rhs._degrees;
    }

    /// <summary>
    /// Imlplicity cast a double as an Angle measured in degrees.
    /// </summary>
    /// <param name="degrees">angle in degrees</param>
    /// <returns>double cast as an Angle</returns>
    public static implicit operator Angle(double degrees)
    {
        return new Angle(degrees);
    }
    #endregion
}
