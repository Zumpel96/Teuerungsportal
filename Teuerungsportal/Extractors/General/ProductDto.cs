namespace Extractors.General;

using System;

public class ProductDto
{
    public Guid id { get; set; }
    
    public string name { get; set; }

    public string articleNumber { get; set; }

    public string url { get; set; }

    public string brand { get; set; }

    public Guid storeId { get; set; }

    public Guid? categoryId { get; set; }

    public bool Equals(ProductDto other)
    {
        return this.name == other.name && this.url == other.url && this.brand == other.brand;
    }
}