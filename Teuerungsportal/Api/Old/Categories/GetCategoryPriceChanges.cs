using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetCategoryPriceChanges
{
    [FunctionName("GetCategoryPriceChanges")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "categories/{categoryId}/prices/{page}")] HttpRequest req,
        [Sql(
                commandText: @"
                                WITH [recursion] AS (
                                  SELECT 
                                    [c].*, 
                                    CAST(
                                      ROW_NUMBER() OVER (
                                        ORDER BY 
                                          [c].[id]
                                      ) AS VARCHAR(MAX)
                                    ) COLLATE Latin1_General_BIN AS [rc] 
                                  FROM 
                                    [dbo].[category] [c] 
                                  WHERE 
                                    LOWER([c].[id]) = LOWER(@categoryId) 
                                  UNION ALL 
                                  SELECT 
                                    [c].*, 
                                    [recursion].[rc] + '.' + CAST(
                                      ROW_NUMBER() OVER (
                                        PARTITION BY [c].[parentId] 
                                        ORDER BY 
                                          [c].[Id]
                                      ) AS VARCHAR(MAX)
                                    ) COLLATE Latin1_General_BIN 
                                  FROM 
                                    [dbo].[category] [c] 
                                    JOIN [recursion] ON [c].[parentID] = [recursion].[id]
                                ), 
                                [recursive_categories] AS (
                                  SELECT 
                                    [r].[id], 
                                    [r].[name], 
                                    [r].[parentId] AS [parentCategoryId], 
                                    [c].[name] AS [parentName], 
                                    [r].[rc] AS [recursionId] 
                                  FROM 
                                    [recursion] [r] 
                                    JOIN [dbo].[category] [c] ON [r].[parentId] = [c].[id]
                                ), 
                                [previous_prices] AS (
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
                                  [p].[productId], 
                                  [p].[productName], 
                                  [p].[articleNumber], 
                                  [p].[brand], 
                                  [p].[storeId], 
                                  [p].[storeName], 
                                  [p].[currentValue], 
                                  [p].[previousValue], 
                                  [p].[timestamp] 
                                FROM 
                                  [previous_prices] [p] 
                                  JOIN [recursive_categories] [c] ON [c].[id] = [p].[categoryId] 
                                WHERE [p].[timestamp] >= DATEADD(day,-30,GETDATE()) AND [p].[previousValue] IS NOT NULL
                                ORDER BY 
                                  [timestamp] DESC
                                OFFSET 
                                  ((@page -1) * 25) ROWS FETCH NEXT 25 ROWS ONLY;",
                parameters: "@categoryId={categoryId},@page={page}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<PriceDbo> prices)
    {
        return new OkObjectResult(prices.Select(dbo => new Price(dbo)));
    }
}