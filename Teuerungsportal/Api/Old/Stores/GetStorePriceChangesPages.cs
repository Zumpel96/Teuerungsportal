using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System;
using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;

public static class GetStorePriceChangesPages
{
    [FunctionName("GetStorePriceChangesPages")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stores/{storeId}/prices")] HttpRequest req,
        [Sql(
                commandText: @"
                                SELECT 
                                   COUNT([p].[id]) AS [count]
                                FROM 
                                  [dbo].[price] [p] 
                                JOIN
                                  [dbo].[product] [pr] ON [p].[productId] = [pr].[id]
                                WHERE 
                                  LOWER([pr].[storeId]) = LOWER(@storeId);",
                parameters: "@storeId={storeId}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<CountDbo> count)
    {
        var countList = count.ToList();
        return !countList.Any() ? new OkObjectResult(0) : new OkObjectResult((int)Math.Ceiling((double)countList.First().Count / 25));
    }
}