using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api.v2;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Teuerungsportal.Models;

public static class GetTotalPriceChangesChartDataForFavorites
{
    private static string Query = @"                          
                                WITH [previous_prices] AS
                                  (SELECT [p].[id],
                                          [p].[productId],
                                          [p].[value] AS [currentValue],
                                          [p].[timestamp],
                                          Lag([p].[value]) OVER (PARTITION BY [p].[productId]
                                                                 ORDER BY [p].[timestamp]) AS [previousValue]
                                   FROM [dbo].[price] [p]
								   WHERE [p].[productId] IN (@productIds)),
                                     [grouped_prices] AS
                                  (SELECT CONVERT(date, [timestamp]) AS [date],
                                          SUM(((100 / [previousValue]) * [currentValue]) - 100) /
                                     (SELECT CASE
                                                 WHEN COUNT(*) > 1 THEN COUNT(*)
                                                 ELSE 1
                                             END AS [count]
                                      FROM [dbo].[product] [pi]) AS [inflationValue]
                                   FROM [previous_prices] [p]
                                   JOIN [dbo].[product] [pr] ON [p].[productId] = [pr].[id]
                                WHERE [previousValue] IS NOT NULL
                                   GROUP BY CONVERT(date, [p].[timestamp]))
                                SELECT [g].[date] AS [date],
                                       [g].[inflationValue] AS [inflationValue]
                                FROM [grouped_prices] [g]
                                ORDER BY [g].[date];";

    [FunctionName("GetTotalPriceChangesChartDataForFavoritesV2")]
    public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/prices/chart/favorites")] HttpRequest req)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        var productIds = JsonConvert.DeserializeObject<List<Guid>>(requestBody);
        if (productIds == null)
        {
            return new OkObjectResult(new List<InflationData>());
        }

        var stringIds = productIds.Select(p => $"'{p}'");
        var alteredQuery = Query.Replace("@productIds", string.Join(',', stringIds));

        await using var connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString"));
        await using var command = new SqlCommand(alteredQuery, connection);

        await connection.OpenAsync();

        await using var reader = await command.ExecuteReaderAsync();
        var inflationData = new List<InflationData>();
        while (await reader.ReadAsync())
        {
            var data = new InflationData()
                        {
                            Date = (DateTime)reader["date"],
                            InflationValue = (double)reader["inflationValue"],
                        };
            inflationData.Add(data);
        }

        return new OkObjectResult(inflationData);
    }
}