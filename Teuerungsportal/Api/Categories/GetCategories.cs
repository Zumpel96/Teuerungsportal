using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetCategories
{
	[FunctionName("GetCategories")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "categories")] HttpRequest req,
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
                                    [parentId] IS NULL 
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
                                  [id], 
                                  [name], 
                                  [rc] AS [recursionId] 
                                FROM 
                                  [recursion] 
                                ORDER BY 
                                  [rc];",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<CategoryDbo> categories)
	{
		var resultCategories = new List<Category>();
		Category.ComputeChildCategories(categories.ToList(), resultCategories, 1);
		return new OkObjectResult(resultCategories.OrderBy(c => c.Name));
    }
}