namespace Shared.DatabaseObjects;

public class PriceDbo
{
    public double CurrentValue { get; set; }

    public double? PreviousValue { get; set; }

    public DateTime Timestamp { get; set; }

    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;
    
    public string ArticleNumber { get; set; } = string.Empty;

    public Guid StoreId { get; set; }

    public string StoreName { get; set; } = string.Empty;

    public string StoreColor { get; set; } = "#FFFFFF";
    
    public string Brand { get; set; } = string.Empty;

    public Guid? CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;
    
}