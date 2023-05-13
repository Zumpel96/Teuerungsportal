namespace Teuerungsportal.Helpers;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ArticleNumber { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string Store { get; set; } = string.Empty;

    public Category? Category { get; set; }
}