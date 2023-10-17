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
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetFavoriteProductsOrderedByAscending
{
    private static string Query = @"                          
                          WITH [previous_prices] AS (
                              SELECT 
                                [p].[id], 
                                [p].[productId], 
                                [pr].[articleNumber], 
                                [pr].[name] AS [productName], 
								                [pr].[brand],
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
                              [brand], 
                              [categoryId], 
                              [categoryName],
                              [storeId],
                              [storeName],
                              [timestamp],
							                [currentValue],
							                [previousValue]
                            FROM 
                              [previous_prices] [p]
							              JOIN [dbo].[store] [s] ON [s].[id] = [p].[storeId]
                            WHERE [s].[hidden] = 0 
                              AND [productId] IN (@productIds)
                            ORDER BY
                              [currentValue] ASC
						                OFFSET ((@page - 1) * 10) ROWS FETCH NEXT 10 ROWS ONLY;";

    [FunctionName("GetFavoriteProductsOrderedByAscendingV2")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/prices/favorite/{page}/ascending")] HttpRequest req)
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