namespace Shared.DatabaseObjects;

public class ProductDbo
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    
    public string ArticleNumber { get; set; } = string.Empty;
    
    public string Url { get; set; } = string.Empty;
    
    public string Brand { get; set; } = string.Empty;
    
    public Guid? CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public Guid? StoreId { get; set; }

    public string StoreName { get; set; } = string.Empty;
    
    public string StoreBaseUrl { get; set; } = string.Empty;
}