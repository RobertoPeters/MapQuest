namespace MapQuest.Models;

public class FilteredDataResult<T> where T : class
{
    public IEnumerable<T> Items { get; set; } = null!;
    public Dictionary<string, double?>? Distances { get; set; }
    public int Count { get; set; }
}
