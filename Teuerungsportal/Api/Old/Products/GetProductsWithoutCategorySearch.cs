using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetProductsWithoutCategorySearch
{
    [FunctionName("GetProductsWithoutCategorySearch")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/noCategory/search/{searchString}/{page}")] HttpRequest req,
      [Sql(
            commandText: @"
                        SELECT [p].[id] AS [productId], 
                               [p].[articleNumber] AS [articleNumber],
                               [p].[name] AS [productName], 
                               [p].[brand] AS [brand], 
                               [c].[id] AS [categoryId], 
                               [c].[name] AS [categoryName],
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
						LEFT JOIN [dbo].[category] [c] ON [p].[categoryId] = [c].[id]
						LEFT JOIN [dbo].[store] [s] ON [p].[storeId] = [s].[id]
						WHERE  [p].[categoryId] IS NULL AND ((LOWER([p].[name]) LIKE LOWER(CONCAT(CHAR(37), @searchString, CHAR(37)))
							OR LOWER([p].[brand]) LIKE LOWER(CONCAT(CHAR(37), @searchString, CHAR(37)))
							OR LOWER([p].[articleNumber]) LIKE LOWER(CONCAT(CHAR(37), @searchString, CHAR(37)))))
						    AND [s].[hidden] = 0						
						GROUP BY [p].[id], [p].[articleNumber], [p].[name], [p].[brand], [c].[id], [prices].[timestamp], [s].[id], [c].[name], [s].[name], [prices].[currentValue], [prices].[previousValue]
						ORDER BY [prices].[currentValue]
						OFFSET ((@page - 1) * 10) ROWS FETCH NEXT 10 ROWS ONLY;",
            parameters: "@page={page},@searchString={searchString}",
            commandType: System.Data.CommandType.Text,
            connectionStringSetting: "SqlConnectionString")]
      IEnumerable<PriceDbo> prices)
    {
      return new OkObjectResult(prices.Select(dbo => new Price(dbo)));
    }
}