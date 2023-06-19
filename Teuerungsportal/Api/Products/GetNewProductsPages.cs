using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System;
using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;

public static class GetNewProductsPages
{
    [FunctionName("GetNewProductsPages")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/new")] HttpRequest req,
        [Sql(
                commandText: @"
                                SELECT 
                                  COUNT(*) AS [count]
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
                                JOIN 
                                  [dbo].[price] [pr] on [pr].[productId] = [p].[id]
                                WHERE 
                                  [c].[priceCount] = 1
                                AND 
                                  [pr].[timestamp] >= DATEADD(day,-1,GETDATE())
                                AND
                                  [s].[hidden] = 0;",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<CountDbo> count)
    {
        var countList = count.ToList();
        return !countList.Any() ? new OkObjectResult(0) : new OkObjectResult((int)Math.Ceiling((double)countList.First().Count / 10));
    }
}