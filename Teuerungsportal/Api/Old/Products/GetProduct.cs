using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetProduct
{
    [FunctionName("GetProduct")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/{articleNumber}/store/{storeName}")] HttpRequest req,
        [Sql(
                commandText: @"
                                SELECT 
                                  [p].[id], 
                                  [p].[name], 
                                  [p].[articleNumber], 
                                  [s].[id] AS [storeId], 
                                  [p].[url], 
                                  [p].[brand], 
                                  [s].[name] AS [storeName], 
                                  [s].[baseUrl] AS [storeBaseUrl], 
                                  [c].[id] AS [categoryId], 
                                  [c].[name] AS [categoryName] 
                                FROM 
                                  [dbo].[product] [p] 
                                  JOIN [dbo].[store] [s] ON [p].[storeId] = [s].[Id ]
                                  LEFT JOIN [dbo].[category] [c] ON [p].[categoryId] = [c].[Id] 
                                WHERE 
                                  LOWER([s].[name]) = LOWER(@storeName) 
                                  AND [s].[hidden] = 0
                                  AND LOWER([p].[articleNumber]) = LOWER(@articleNumber);",
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

        return new OkObjectResult(new Product(productsList.First()));
    }
}