using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System;
using System.Collections.Generic;
using System.Linq;

public static class GetProductsWithoutCategory
{
    public class ProductDbo
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string ArticleNumber { get; set; }

        public Guid StoreId { get; set; }

        public string StoreName { get; set; }

        public string StoreBaseUrl { get; set; }

        public string Url { get; set; }

        public string Brand { get; set; }
    }

    public class Product
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string ArticleNumber { get; set; }

        public string Url { get; set; }

        public string Brand { get; set; }

        public Store Store { get; set; }
    }

    public class Store
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string BaseUrl { get; set; }
    }

    [FunctionName("GetProductsWithoutCategory")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/noCategory/{page}")] HttpRequest req,
        [Sql(
                commandText: @"SELECT p.id, p.name, p.articleNumber, s.id AS 'storeId', p.url, p.brand,  
                                        s.name AS 'storeName', s.baseUrl AS 'storeBaseUrl'
				 				 FROM [dbo].[product] p 
				 				 JOIN [dbo].[store] s ON p.storeId = s.Id 
				 				 WHERE p.categoryId IS NULL
                                 ORDER BY p.name
                                 OFFSET ((@page - 1) * 25) ROWS
                                 FETCH NEXT 25 ROWS ONLY;",
                parameters: "@page={page}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<ProductDbo> products)
    {
        return new OkObjectResult(
                                  products.Select(
                                                  p => new Product()
                                                       {
                                                           Brand = p.Brand,
                                                           Id = p.Id,
                                                           Name = p.Name,
                                                           ArticleNumber = p.ArticleNumber,
                                                           Url = p.Url,
                                                           Store = new Store()
                                                                   {
                                                                       Id = p.StoreId,
                                                                       Name = p.StoreName,
                                                                       BaseUrl = p.StoreBaseUrl,
                                                                   }
                                                       }));
    }
}