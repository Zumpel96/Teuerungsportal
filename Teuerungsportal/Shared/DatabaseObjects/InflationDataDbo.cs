namespace Shared.DatabaseObjects;

public class InflationDataDbo
{
    public double InflationValue { get; set; }

    public DateTime Date { get; set; }

    public Guid StoreId { get; set; }

    public string StoreName { get; set; } = string.Empty;
    
    public string StoreColor { get; set; } = "#FFFFFF";

}