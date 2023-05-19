using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetCategory
{
    [FunctionName("GetCategory")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "categories/{categoryName}")] HttpRequest req,
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
                                    LOWER([c].[name]) = LOWER(@categoryName) 
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
                                ) 
                                SELECT 
                                  [r].[id], 
                                  [r].[name], 
                                  [r].[parentId] AS [parentCategoryId], 
                                  [c].[name] AS [parentName], 
                                  [r].[rc] AS [recursionId] 
                                FROM 
                                  [recursion] [r] 
                                  LEFT JOIN [dbo].[category] [c] ON [r].[parentId] = [c].[id] 
                                ORDER BY 
                                  [r].[rc];",
                parameters: "@categoryName={categoryName}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<CategoryDbo> categories)
    {
        var categoryList = categories.ToList();
        if (!categoryList.Any())
        {
            return new NotFoundResult();
        }

        var resultCategories = new List<Category>();
        Category.ComputeChildCategories(categoryList.ToList(), resultCategories, 1);
        return new OkObjectResult(resultCategories.First());
    }
}