namespace Shared.DatabaseObjects;

public class FilteredCountDbo
{
    public string StoreName { get; set; } = string.Empty;
    
    public double Count { get; set; }  
    
    public double IncreasedCount { get; set; }    
    
    public double DecreasedCount { get; set; }
    
    public double NewCount { get; set; }
}