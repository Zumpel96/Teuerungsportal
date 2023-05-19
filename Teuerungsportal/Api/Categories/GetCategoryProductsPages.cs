using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System;
using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;

public static class GetCategoryProductsPages
{
    [FunctionName("GetCategoryProductsPages")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "categories/{categoryId}/products")] HttpRequest req,
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
                                    [r].[id]
                                  FROM 
                                    [recursion] [r] 
                                    JOIN [dbo].[category] [c] ON [r].[parentId] = [c].[id]
                                )
                                SELECT 
                                  COUNT(*) as [count]
                                FROM 
                                  [dbo].[product] [p] 
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