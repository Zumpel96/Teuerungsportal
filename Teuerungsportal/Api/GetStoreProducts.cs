using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System;
using System.Collections.Generic;
using System.Linq;

public static class GetStoreProducts
{
    public class ProductDbo
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string ArticleNumber { get; set; }

        public string Url { get; set; }

        public string Brand { get; set; }

        public Guid? CategoryId { get; set; }

        public string CategoryName { get; set; }
    }

    public class Product
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string ArticleNumber { get; set; }

        public string Url { get; set; }

        public string Brand { get; set; }

        public Category Category { get; set; }
    }

    public class Category
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }

    [FunctionName("GetStoreProducts")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "store/{storeId}/products")] HttpRequest req,
        [Sql(
                commandText: @"SELECT p.id, p.name, p.articleNumber, p.url, p.brand, c.id AS categoryId, c.name AS categoryName
				 				 FROM [dbo].[product] p
				 				 LEFT JOIN [dbo].[category] c ON p.categoryId = c.Id
				 				 WHERE LOWER(p.storeId) = LOWER(@storeId);",
                parameters: "@storeId={storeId}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<ProductDbo> products)
    {
        return new OkObjectResult(
                                  products.Select(
                                                  p => new Product()
                                                       {
                                                           Id = p.Id,
                                                           Name = p.Name,
                                                           ArticleNumber = p.ArticleNumber,
                                                           Brand = p.Brand,
                                                           Url = p.Url,
                                                           Category = p.CategoryId == null
                                                                      ? null
                                                                      : new Category()
                                                                        {
                                                                            Id = (Guid)p.CategoryId,
                                                                            Name = p.CategoryName,
                                                                        },
                                                       }));
    }
}