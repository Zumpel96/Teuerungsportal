using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System.Collections.Generic;
using System.Linq;

public static class GetProduct
{
    public class ProductDbo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ArticleNumber { get; set; }

        public int StoreId { get; set; }

        public string StoreName { get; set; }

        public string StoreBaseUrl { get; set; }

        public string Url { get; set; }

        public string Brand { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ArticleNumber { get; set; }

        public string Url { get; set; }

        public string Brand { get; set; }

        public Store Store { get; set; }

        public Category Category { get; set; }
    }

    public class Store
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string BaseUrl { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    [FunctionName("GetSpecificProduct")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stores/{storeName}/{articleNumber}")] HttpRequest req,
        [Sql(
                commandText: @"SELECT p.id, p.name, p.articleNumber, s.id AS 'storeId', p.url, p.brand,  
                                        s.name AS 'storeName', s.baseUrl AS 'storeBaseUrl', c.id AS 'categoryId', c.name AS 'categoryName'
				 				 FROM [dbo].[product] p 
				 				 JOIN [dbo].[store] s ON p.storeId = s.Id 
				 				 JOIN [dbo].[category] c ON p.categoryId = c.Id 
				 				 WHERE LOWER(s.name) = LOWER(@storeName) AND LOWER(p.articleNumber) = LOWER(@articleNumber);",
                parameters: "@storeName={storeName},@articleNumber={articleNumber}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<ProductDbo> products)
    {
        var productsList = products.ToList();
        if (!productsList.Any())
        {
            return new NotFoundResult();
        }

        var foundProduct = productsList.First();
        return new OkObjectResult(
                                  new Product()
                                  {
                                      Brand = foundProduct.Brand,
                                      Id = foundProduct.Id,
                                      Name = foundProduct.Name,
                                      ArticleNumber = foundProduct.ArticleNumber,
                                      Url = foundProduct.Url,
                                      Store = new Store()
                                              {
                                                  Id = foundProduct.StoreId,
                                                  Name = foundProduct.StoreName,
                                                  BaseUrl = foundProduct.StoreBaseUrl,
                                              },
                                      Category = new Category()
                                                 {
                                                     Id = foundProduct.CategoryId,
                                                     Name = foundProduct.CategoryName,
                                                 },
                                  });
    }
}