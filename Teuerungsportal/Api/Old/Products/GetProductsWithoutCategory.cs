using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetProductsWithoutCategory
{
    [FunctionName("GetProductsWithoutCategory")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/noCategory/{page}")] HttpRequest req,
        [Sql(
                commandText: @"
                        SELECT [p].[id] AS [productId], 
                               [p].[articleNumber] AS [articleNumber],
                               [p].[name] AS [productName], 
                               [p].[brand] AS [brand],
                               [s].[id] AS [storeId],
                               [s].[name] AS [storeName],
                               [prices].[timestamp] AS [timestamp],
							   [prices].[currentValue] AS [currentValue],
							   [prices].[previousValue] AS [previousValue]
						FROM [dbo].[product] [p]
						LEFT JOIN (
							SELECT
								[productId],
								[timestamp],
								[value] AS [currentValue],
								LAG([value]) OVER (PARTITION BY [productId] ORDER BY [timestamp]) AS [previousValue]
							FROM [dbo].[price]
						) [prices] ON [prices].[productId] = [p].[id]
						LEFT JOIN [dbo].[store] [s] ON [p].[storeId] = [s].[id]
						WHERE [p].[categoryId] IS NULL
						    AND [s].[hidden] = 0						
						GROUP BY [p].[id], [p].[articleNumber], [p].[name], [p].[brand], [prices].[timestamp], [s].[id], [s].[name], [prices].[currentValue], [prices].[previousValue]
						ORDER BY [p].[name]
						OFFSET ((@page - 1) * 10) ROWS FETCH NEXT 10 ROWS ONLY;",
                parameters: "@page={page}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<PriceDbo> prices)
    {
        return new OkObjectResult(prices.Select(dbo => new Price(dbo)));
    }
}