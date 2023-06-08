using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetProductsWithoutCategory
{
    [FunctionName("GetProductsWithoutCategory")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/noCategory/{page}")] HttpRequest req,
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
                                  [s].[baseUrl] AS [storeBaseUrl] 
                                FROM 
                                  [dbo].[product] [p] 
                                  JOIN [dbo].[store] [s] ON [p].[storeId] = [s].[Id] 
                                WHERE 
                                  [p].[categoryId] IS NULL 
                                AND
                                  [s].[hidden] = 0
                                ORDER BY 
                                  [p].[name] 
                                OFFSET 
                                  ((@page -1) * 25) ROWS FETCH NEXT 25 ROWS ONLY;",
                parameters: "@page={page}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<ProductDbo> products)
    {
        return new OkObjectResult(products.Select(dbo => new Product(dbo)));
    }
}