using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System;
using System.Collections.Generic;
using System.Linq;

public static class GetCategoryPriceChanges
{
    public class PriceDbo
    {
        public Guid ProductId { get; set; }

        public string ProductName { get; set; }

        public string Brand { get; set; }

        public Guid StoreId { get; set; }

        public string StoreName { get; set; }

        public double Value { get; set; }

        public DateTime TimeStamp { get; set; }
    }

    public class Price
    {
        public double Value { get; set; }

        public DateTime TimeStamp { get; set; }

        public Product Product { get; set; }
    }

    public class Store
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
    
    public class Product
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Brand { get; set; }

        public Store Store { get; set; }
    }

    [FunctionName("GetPriceChangesForCategory")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "prices/category/{categoryId}")] HttpRequest req,
        [Sql(
                commandText: @"WITH category_tree AS (
                                  SELECT id, parentId
                                  FROM [dbo].[category]
                                  WHERE id = LOWER(@categoryId)
                                  
                                  UNION ALL
                                  
                                  SELECT c.id, c.parentId
                                  FROM [dbo].[category] c
                                  INNER JOIN category_tree ct ON ct.id = c.parentId
                                ),
                                product_counts AS (
                                  SELECT c.id AS category_id, COUNT(p.id) AS product_count
                                  FROM category_tree ct
                                  INNER JOIN [dbo].[category] c ON ct.id = c.id
                                  LEFT JOIN [dbo].[product] p ON c.id = p.categoryId
                                  GROUP BY c.id
                                )
                                SELECT p.id AS productId, p.name AS productName, p.brand, pc.category_id, s.id AS storeId, s.name AS storeName, c.name AS categoryName, pr.value AS value, pr.timestamp AS timestamp
                                FROM product_counts pc
                                INNER JOIN [dbo].[category] c ON pc.category_id = c.id
                                LEFT JOIN [dbo].[product] p ON c.id = p.categoryId
                                LEFT JOIN [dbo].[store] s ON s.id = p.storeId
                                LEFT JOIN [dbo].[price] pr ON p.id = pr.productId
                                WHERE pc.product_count > 0
                                ORDER BY pc.category_id, pr.value;",
                parameters: "@categoryId={categoryId}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<PriceDbo> prices)
    {
        return new OkObjectResult(
                                  prices.Select(
                                                p => new Price()
                                                     {
                                                         Value = p.Value,
                                                         TimeStamp = p.TimeStamp,
                                                         Product = new Product()
                                                                   {
                                                                       Id = p.ProductId,
                                                                       Name = p.ProductName,
                                                                       Brand = p.Brand,
                                                                       Store = new Store()
                                                                               {
                                                                                   Id = p.StoreId,
                                                                                   Name = p.StoreName,
                                                                               }
                                                                   }
                                                     }).
                                         ToList());
    }
}