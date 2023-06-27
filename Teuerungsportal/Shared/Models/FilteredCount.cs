namespace Teuerungsportal.Models;

using Shared.DatabaseObjects;

public class FilteredCount
{
    public string StoreName { get; set; }

    public double Count { get; set; }

    public double IncreasedCount { get; set; }

    public double DecreasedCount { get; set; }

    public double NewCount { get; set; }

    public FilteredCount()
    {
        this.StoreName = string.Empty;
        this.Count = 0;
        this.IncreasedCount = 0;
        this.DecreasedCount = 0;
        this.NewCount = 0;
    }

    public FilteredCount(FilteredCountDbo dbo)
    {
        this.StoreName = dbo.StoreName;
        this.Count = dbo.Count;
        this.IncreasedCount = dbo.IncreasedCount;
        this.DecreasedCount = dbo.DecreasedCount;
        this.NewCount = dbo.NewCount;
    }
}