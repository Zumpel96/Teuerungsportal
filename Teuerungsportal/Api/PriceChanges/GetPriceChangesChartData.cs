using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api.v2;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetPriceChangesChartData
{
    [FunctionName("GetPriceChangesChartDataV2")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v2/prices/chart/month")] HttpRequest req,
        [Sql(
                commandText: @"
                                WITH [previous_prices] AS
                                  (SELECT [p].[id],
                                          [p].[productId],
                                          [p].[value] AS [currentValue],
                                          [p].[timestamp],
                                          Lag([p].[value]) OVER (PARTITION BY [p].[productId]
                                                                 ORDER BY [p].[timestamp]) AS [previousValue]
                                   FROM [dbo].[price] [p]),
                                     [grouped_prices] AS
                                  (SELECT [pr].[storeId],
                                          CONVERT(date, [timestamp]) AS [date],
                                          SUM(((100 / [previousValue]) * [currentValue]) - 100) /
                                     (SELECT CASE
                                                 WHEN COUNT(*) > 1 THEN COUNT(*)
                                                 ELSE 1
                                             END AS [count]
                                      FROM [dbo].[product] [pi]
                                      WHERE [pi].[storeId] = [pr].[storeId]) AS [inflationValue]
                                   FROM [previous_prices] [p]
                                   JOIN [dbo].[product] [pr] ON [p].[productId] = [pr].[id]
                                   JOIN [dbo].[store] [s] ON [pr].[storeId] = [s].[id]
                                   WHERE [timestamp] >= DATEADD(DAY, -30, GETDATE())
                                     AND [s].[hidden] = 0
                                     AND [previousValue] IS NOT NULL
                                   GROUP BY [pr].[storeId],
                                            CONVERT(date, [p].[timestamp]))
                                SELECT [g].[storeId] AS [storeId],
                                       [s].[name] AS [storeName],
                                       [g].[date] AS [date],
                                       [g].[inflationValue] AS [inflationValue]
                                FROM [grouped_prices] [g]
                                JOIN [dbo].[store] [s] ON [s].[id] = [g].[storeId]
                                ORDER BY [g].[date];",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<InflationDataDbo> inflationObjects)
    {
        return new OkObjectResult(inflationObjects.Select(dbo => new InflationData(dbo)));
    }
}