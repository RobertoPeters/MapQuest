namespace MapQuest.Models;

public class FilteredDataRequest
{
    public int? Skip { get; set; }
    public int? Take { get; set; }
    public bool IgnoreCount { get; set; }
    public double? Lat { get; set; }
    public double? Lon { get; set; }
    public IEnumerable<(string ColumnName, object? Value)>? Filter { get; set; }
}
