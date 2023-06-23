using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetTopPriceChanges
{
    [FunctionName("GetTopPriceChanges")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "prices/top")] HttpRequest req,
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
								  TOP (5)
                                  [productId], 
                                  [productName], 
                                  [articleNumber], 
                                  [categoryId], 
                                  [categoryName],                                  
                                  [storeId], 
                                  [storeName], 
                                  [currentValue], 
                                  [previousValue], 
                                  ((100 / [previousValue]) * [currentValue]) AS [difference],
                                  [timestamp] 
                                FROM 
                                  [previous_prices] 
                                WHERE [timestamp] >= DATEADD(day,-1,GETDATE()) AND [previousValue] IS NOT NULL AND [previousValue] > [currentValue]
                                ORDER BY 
                                  [difference] ASC;",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<PriceDbo> prices)
    {
        return new OkObjectResult(prices.Select(dbo => new Price(dbo)));
    }
}