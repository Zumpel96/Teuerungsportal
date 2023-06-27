using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api.v2;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetNumberOfPriceChangesYearly
{
    [FunctionName("GetNumberOfPriceChangesYearlyV2")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v2/prices/total/year")] HttpRequest req,
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
                              [name] AS [StoreName],
                              [color] AS [StoreColor], 
                              (
                                SELECT 
                                  COUNT([id]) 
                                FROM 
                                  (
                                    SELECT 
                                      [id], 
                                      [storeId] 
                                    FROM 
                                      [previous_prices]
                                    WHERE 
                                      [timestamp] >= CAST(GETDATE() - 364 AS DATE)
		                              AND [previousValue] IS NOT NULL
                                  ) AS sub 
                                WHERE 
                                  [sub].[storeId] = [s].[id]
                              ) AS [Count]
                            FROM 
                              [dbo].[store] [s]
                            WHERE [s].[hidden] = 0
                            GROUP BY 
                              [s].[name], [s].[color], [s].[id];",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<FilteredCountDbo> products)
    {
     return new OkObjectResult(products.Select(dbo => new FilteredCount(dbo)));
    }
}