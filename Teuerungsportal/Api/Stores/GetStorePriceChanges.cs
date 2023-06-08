using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetStorePriceChanges
{
    [FunctionName("GetStorePriceChanges")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stores/{storeId}/prices")] HttpRequest req,
        [Sql(
                commandText: @"
                                WITH [previous_prices] AS (
                                  SELECT 
                                    [p].[id], 
                                    [p].[productId], 
                                    [pr].[articleNumber], 
                                    [pr].[name] AS [productName], 
                                    [pr].[storeId], 
                                    [s].[name] AS [storeName], 
                                    [pr].[categoryId] AS [categoryId], 
                                    [c].[name] AS [categoryName], 
                                    [p].[value] AS [currentValue], 
                                    [p].[timestamp], 
                                    Lag([p].[value]) OVER (
                                      partition BY [p].[productId] 
                                      ORDER BY 
                                        [p].[timestamp]
                                    ) AS [previousValue] 
                                  FROM 
                                    [dbo].[price] [p] 
                                    JOIN [dbo].[product] [pr] ON [p].[productId] = [pr].[id] 
                                    JOIN [dbo].[store] [s] ON [pr].[storeId] = [s].[id] 
                                    LEFT JOIN [dbo].[category] [c] ON [pr].[categoryId] = [c].[id]
                                  WHERE
                                    [s].[hidden] = 0
                                ) 
                                SELECT 
                                  [productId], 
                                  [articleNumber], 
                                  [productName], 
                                  [categoryId], 
                                  [categoryName], 
                                  [storeId], 
                                  [storeName], 
                                  [currentValue], 
                                  [previousValue], 
                                  [timestamp] 
                                FROM 
                                  [previous_prices] 
                                WHERE 
                                  LOWER([storeId]) = LOWER(@storeId) AND [timestamp] >= DATEADD(day,-30,GETDATE()) AND [previousValue] IS NOT NULL
                                ORDER BY 
                                  [timestamp] DESC;",
                parameters: "@storeId={storeId}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<PriceDbo> prices)
    {
        return new OkObjectResult(prices.Select(dbo => new Price(dbo)));
    }
}