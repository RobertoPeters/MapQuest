using System.ComponentModel.DataAnnotations;

namespace MapQuest.Geo;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class CoordinateAttribute : ValidationAttribute
{
    public CoordinateAttribute()
    {

    }

    public override bool IsValid(object? value)
    {
        return GeoService.StringToLocation(value as string).Lat != null;
    }
}
