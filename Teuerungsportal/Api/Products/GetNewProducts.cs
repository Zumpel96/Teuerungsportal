using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetNewProducts
{
    [FunctionName("GetNewProducts")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/new/{page}")] HttpRequest req,
        [Sql(
                commandText: @"
                                SELECT 
                                  [p].[id] AS [id], 
                                  [p].[name] AS [name], 
                                  [p].[articleNumber] AS [articleNumber], 
                                  [cat].[id] AS [categoryId], 
                                  [cat].[name] AS [categoryName], 
                                  [p].[url] AS [url], 
                                  [p].[brand] AS [brand], 
                                  [s].[id] AS [storeId], 
                                  [s].[name] AS [storeName], 
                                  [s].[baseUrl] AS [storeBaseUrl]
                                FROM 
                                  (
                                    SELECT 
                                      [productId], 
                                      COUNT(*) AS priceCount 
                                    FROM 
                                      [dbo].[price]
                                    WHERE 
                                      [timestamp] >= DATEADD(day, -1, GETDATE()) 
                                    GROUP BY 
                                      [productId]
                                  ) [c]
                                JOIN 
                                  [dbo].[product] [p] ON [p].[id] = [c].[productId]
                                JOIN
                                  [dbo].[store] [s] ON [s].[id] = [p].[storeId]
                                LEFT JOIN
                                  [dbo].[category] [cat] ON [cat].[id] = [p].[categoryId]
                                WHERE 
                                  [c].[priceCount] = 1
                                AND
                                  [s].[hidden] = 0
                                ORDER BY 
                                  [name] 
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