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
                             WITH [previous_prices] AS
                                  (SELECT [p].[id],
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
                                          Lag([p].[value]) OVER (PARTITION BY [p].[productId]
                                                                 ORDER BY [p].[timestamp]) AS [previousValue]
                                   FROM [dbo].[price] [p]
                                   JOIN [dbo].[product] [pr] ON [p].[productId] = [pr].[id]
                                   JOIN [dbo].[store] [s] ON [pr].[storeId] = [s].[id]
                                   LEFT JOIN [dbo].[category] [c] ON [pr].[categoryId] = [c].[id]
                                   WHERE [s].[hidden] = 0 )
                                SELECT COUNT(*) AS [count]
                                FROM [GetCategoryProducts](@categoryId) [c]
								JOIN [previous_prices] [p] ON [p].[productId] = [c].[id];",
                parameters: "@categoryId={categoryId}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
      IEnumerable<CountDbo> count)
    {
      var countList = count.ToList();
      return !countList.Any() ? new OkObjectResult(0) : new OkObjectResult((int)Math.Ceiling((double)countList.First().Count / 25));
    }
}