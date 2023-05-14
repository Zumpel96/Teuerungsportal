namespace Teuerungsportal.Helpers;

public class Product
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ArticleNumber { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public Store? Store { get; set; }

    public Category? Category { get; set; }
}