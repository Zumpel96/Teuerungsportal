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

public static class GetNumberOfPriceChangesYearlyForFavorites
{
    private static string Query = @"                          
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
                              AND [pr].[id] IN (@productIds)
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
                                    [previousValue] IS NOT NULL
									                  AND [timestamp] >= CAST(GETDATE() - 364 AS DATE)
                                ) AS sub 
                              WHERE 
                                [sub].[storeId] = [s].[id]
                            ) AS [Count], 
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
                                    [previousValue] IS NOT NULL 
									                  AND [timestamp] >= CAST(GETDATE() - 364 AS DATE)
                                    AND [previousValue] < [currentValue]
                                ) AS sub 
                              WHERE 
                                [sub].[storeId] = [s].[id]
                            ) AS [IncreasedCount], 
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
                                    [previousValue] IS NOT NULL 
									                  AND [timestamp] >= CAST(GETDATE() - 364 AS DATE)
                                    AND [previousValue] > [currentValue]
                                ) AS sub 
                              WHERE 
                                [sub].[storeId] = [s].[id]
                            ) AS [DecreasedCount] 
                          FROM 
                            [dbo].[store] [s] 
                          WHERE 
                            [s].[hidden] = 0 
                          GROUP BY 
                            [s].[name], 
                            [s].[color], 
                            [s].[id];";

    [FunctionName("GetNumberOfPriceChangesYearlyForFavoritesV2")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/prices/favorites/week")] HttpRequest req)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        
        var productIds = JsonConvert.DeserializeObject<List<Guid>>(requestBody);
        if (productIds == null)
        {
          return new OkObjectResult(new List<FilteredCount>());
        }
        
        var stringIds = productIds.Select(p => $"'{p}'");
        var alteredQuery = Query.Replace("@productIds", string.Join(',', stringIds));
        
        await using var connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString"));
        await using var command = new SqlCommand(alteredQuery, connection);
        
        await connection.OpenAsync();

        await using var reader = await command.ExecuteReaderAsync();
        var counts = new List<FilteredCount>();
        while (await reader.ReadAsync())
        {
            var count = new FilteredCount()
                        {
                            Store = new Store()
                                    {
                                        Name = reader["StoreName"].ToString() ?? string.Empty,
                                        Color = reader["StoreColor"].ToString() ?? string.Empty,
                                    },
                            Count = (int)reader["Count"],
                            IncreasedCount = (int)reader["IncreasedCount"],
                            DecreasedCount = (int)reader["DecreasedCount"],
                        };
            counts.Add(count);
        }

        return new OkObjectResult(counts);
    }
}