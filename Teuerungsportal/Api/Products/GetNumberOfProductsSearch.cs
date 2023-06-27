using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api.v2;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetNumberOfProductsSearch
{
    [FunctionName("GetNumberOfProductsSearchV2")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v2/products/total/search/{searchString}")] HttpRequest req,
        [Sql(
                commandText: @"
                          WITH [previous_prices] AS (
                            SELECT 
                              [p].[id], 
                              [p].[productId], 
                              [pr].[articleNumber], 
                              [pr].[name] AS [productName], 
                              [pr].[storeId], 
                              [pr].[brand], 
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
                              AND (
                                (
                                  LOWER([pr].[name]) LIKE LOWER(CONCAT(CHAR(37), @searchString, CHAR(37))) 
                                  OR LOWER([pr].[brand]) LIKE LOWER(CONCAT(CHAR(37), @searchString, CHAR(37))) 
                                  OR LOWER([pr].[articleNumber]) LIKE LOWER(CONCAT(CHAR(37), @searchString, CHAR(37)))
                                )
                              )
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
                                    [previousValue] < [currentValue]
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
                                    [previousValue] > [currentValue]
                                ) AS sub 
                              WHERE 
                                [sub].[storeId] = [s].[id]
                            ) AS [DecreasedCount], 
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
                                    [previousValue] IS NULL
                                ) AS sub 
                              WHERE 
                                [sub].[storeId] = [s].[id]
                            ) AS [NewCount] 
                          FROM 
                            [dbo].[store] [s] 
                          WHERE 
                            [s].[hidden] = 0 
                          GROUP BY 
                            [s].[name], 
                            [s].[color], 
                            [s].[id];",
                parameters: "@searchString={searchString}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<FilteredCountDbo> products)
    {
     return new OkObjectResult(products.Select(dbo => new FilteredCount(dbo)));
    }
}