namespace Teuerungsportal.Models;

using Shared.DatabaseObjects;

public class FilteredCount
{
    public string StoreName { get; set; }

    public double Count { get; set; }

    public FilteredCount()
    {
        this.StoreName = string.Empty;
        this.Count = 0;
    }

    public FilteredCount(FilteredCountDbo dbo)
    {
        this.StoreName = dbo.StoreName;
        this.Count = dbo.Count;
    }
}