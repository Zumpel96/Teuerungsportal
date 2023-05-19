using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System;
using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;

public static class GetCategoryPriceChangesPages
{
    [FunctionName("GetCategoryPriceChangesPages")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "categories/{categoryId}/prices")] HttpRequest req,
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
                                    [pr].[categoryId] AS [categoryId]
                                  FROM 
                                    [dbo].[price] [p] 
                                    JOIN [dbo].[product] [pr] ON [p].[productId] = [pr].[id] 
                                ) 
                                SELECT 
                                  COUNT(*) AS [count] 
                                FROM 
                                  [previous_prices] [p] 
                                  JOIN [recursive_categories] [c] ON [c].[id] = [p].[categoryId];",
                parameters: "@categoryId={categoryId}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<CountDbo> count)
    {
      var countList = count.ToList();
      return !countList.Any() ? new OkObjectResult(0) : new OkObjectResult((int)Math.Ceiling((double)countList.First().Count / 25));
    }
}