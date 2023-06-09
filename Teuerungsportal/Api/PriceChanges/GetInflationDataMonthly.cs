using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api.v2;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetInflationDataMonthly
{
    [FunctionName("GetInflationDataMonthlyV2")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v2/prices/inflation/total/month")] HttpRequest req,
        [Sql(
                commandText: @"
                         WITH [previous_prices] AS (
                          SELECT 
                            [p].[id], 
                            [p].[productId], 
                            [p].[value] AS [currentValue], 
                            [p].[timestamp], 
                            Lag([p].[value]) OVER (
                              PARTITION BY [p].[productId] 
                              ORDER BY 
                                [p].[timestamp]
                            ) AS [previousValue] 
                          FROM 
                            [dbo].[price] [p]
                        ), 
                        [grouped_prices] AS (
                          SELECT 
                            [pr].[storeId], 
                            SUM(
                              (
                                (100 / [previousValue]) * [currentValue]) -100
                              ) / (
                              SELECT 
                                CASE WHEN COUNT(*) > 1 THEN COUNT(*) ELSE 1 END AS [count] 
                              FROM 
                                [dbo].[product] [pi] 
                              WHERE 
                                [pi].[storeId] = [pr].[storeId]
                            ) AS [inflationValue] 
                          FROM 
                            [previous_prices] [p] 
                            JOIN [dbo].[product] [pr] ON [p].[productId] = [pr].[id] 
                            JOIN [dbo].[store] [s] ON [pr].[storeId] = [s].[id] 
                          WHERE 
                            [timestamp] >= CAST(GETDATE() - 29 AS DATE) 
                            AND [s].[hidden] = 0 
                            AND [previousValue] IS NOT NULL 
                          GROUP BY 
                            [pr].[storeId]
                        ) 
                        SELECT 
                          [s].[name] AS [StoreName], 
                          [s].[color] AS [StoreColor], 
                          [g].[inflationValue] AS [Count] 
                        FROM 
                          [grouped_prices] [g] 
                          JOIN [dbo].[store] [s] ON [s].[id] = [g].[storeId];",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
      IEnumerable<FilteredCountDbo> inflationObjects)
    {
      return new OkObjectResult(inflationObjects.Select(dbo => new FilteredCount(dbo)));
    }
}