namespace Teuerungsportal.Models;

using Shared.DatabaseObjects;

public class Price
{
    public double CurrentValue { get; set; }

    public double? PreviousValue { get; set; }

    public DateTime TimeStamp { get; set; }

    public Product? Product { get; set; }

    public Price()
    {
    }

    public Price(PriceDbo dbo)
    {
        this.CurrentValue = dbo.CurrentValue;
        this.PreviousValue = dbo.PreviousValue;
        this.TimeStamp = dbo.Timestamp;

        this.Product = new Product()
                       {
                           Id = dbo.ProductId,
                           Name = dbo.ProductName,
                           Brand = dbo.Brand,
                           ArticleNumber = dbo.ArticleNumber,
                           Store = new Store()
                                   {
                                       Id = dbo.StoreId,
                                       Name = dbo.StoreName,
                                       Color = dbo.StoreColor,
                                   },
                           Category = dbo.CategoryId == null
                                      ? null
                                      : new Category()
                                        {
                                            Id = (Guid)dbo.CategoryId,
                                            Name = dbo.CategoryName,
                                        }
                       };
    }
}