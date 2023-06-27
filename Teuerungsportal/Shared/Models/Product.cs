namespace Teuerungsportal.Models;

using Shared.DatabaseObjects;

public class Product
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string ArticleNumber { get; set; }

    public string Url { get; set; }

    public string Brand { get; set; }

    public Store? Store { get; set; }

    public Category? Category { get; set; }

    public Product()
    {
        this.Name = string.Empty;
        this.ArticleNumber = string.Empty;
        this.Url = string.Empty;
        this.Brand = string.Empty;
    }
    
    public Product(ProductDbo dbo)
    {
        this.Id = dbo.Id;
        this.Name = dbo.Name;
        this.ArticleNumber = dbo.ArticleNumber;
        this.Url = dbo.Url;
        this.Brand = dbo.Brand;

        this.Store = dbo.StoreId == null
                     ? null
                     : new Store()
                       {
                           Id = (Guid)dbo.StoreId,
                           Name = dbo.StoreName,
                           BaseUrl = dbo.StoreBaseUrl,
                           Color = dbo.StoreColor,
                       };

        this.Category = dbo.CategoryId == null
                        ? null
                        : new Category()
                          {
                              Id = (Guid)dbo.CategoryId,
                              Name = dbo.CategoryName,
                          };
    }
}