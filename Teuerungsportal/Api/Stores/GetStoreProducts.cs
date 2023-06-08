using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetStoreProducts
{
    [FunctionName("GetStoreProducts")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stores/{storeId}/products/{page}")] HttpRequest req,
        [Sql(
                commandText: @"
                                SELECT 
                                  [p].[id], 
                                  [p].[name], 
                                  [p].[articleNumber], 
                                  [p].[url], 
                                  [p].[brand], 
                                  [s].[id] AS [storeId],
                                  [s].[name] AS [storeName],
                                  [c].[id] AS [categoryId], 
                                  [c].[name] AS [categoryName] 
                                FROM 
                                  [dbo].[product] [p] 
                                  JOIN [dbo].[store] [s] ON [p].[storeId] = [s].[id]
                                  LEFT JOIN [dbo].[category] [c] ON [p].[categoryId] = [c].[Id] 
                                WHERE 
                                  LOWER([p].[storeId]) = LOWER(@storeId)
                                AND
                                  [s].[hidden] = 0
                                ORDER BY 
                                  [p].[name]
                                OFFSET 
                                  ((@page -1) * 25) ROWS FETCH NEXT 25 ROWS ONLY;",
                parameters: "@storeId={storeId},@page={page}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<ProductDbo> products)
    {
        return new OkObjectResult(products.Select(dbo => new Product(dbo)));
    }
}