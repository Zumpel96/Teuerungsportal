namespace Shared.DatabaseObjects;

public class FilteredCountDbo
{
    public string StoreName { get; set; } = string.Empty;
    
    public string StoreColor { get; set; } = "#FFFFFF";
    
    public double Count { get; set; }  
    
    public double IncreasedCount { get; set; }    
    
    public double DecreasedCount { get; set; }
    
    public double NewCount { get; set; }
}