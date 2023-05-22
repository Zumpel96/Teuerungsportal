using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetCategoriesUngrouped
{
	[FunctionName("GetCategoriesUngrouped")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "categories/ungrouped")] HttpRequest req,
        [Sql(
                commandText: @"
                                SELECT 
                                  [id], 
                                  [name]
                                FROM 
                                  [dbo].[category] 
                                ORDER BY 
                                  [name];",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<CategoryDbo> categories)
	{
        return new OkObjectResult(categories.Select(c => new Category(c)));
    }
}