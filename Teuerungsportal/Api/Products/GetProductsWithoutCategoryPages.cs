using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System;
using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;

public static class GetProductsWithoutCategoryPages
{
    [FunctionName("GetProductsWithoutCategoryPages")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/noCategory")] HttpRequest req,
        [Sql(
                commandText: @"
                                SELECT 
                                  COUNT(*) AS [count] 
                                FROM 
                                  [dbo].[product] [p]
                                JOIN 
                                  [dbo].[store] [s] ON [p].[storeId] = [s].[Id] 
                                WHERE 
                                  [p].[categoryId] IS NULL
                                AND
                                  [s].[hidden] = 0;",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<CountDbo> count)
    {
        var countList = count.ToList();
        return !countList.Any() ? new OkObjectResult(0) : new OkObjectResult((int)Math.Ceiling((double)countList.First().Count / 25));
    }
}