using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetNewProducts
{
    [FunctionName("GetNewProducts")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/new/{page}")] HttpRequest req,
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
                               [pr].[timestamp] AS [timestamp],
							   [pr].[value] AS [currentValue]
						FROM (
                                    SELECT 
                                      [productId], 
                                      COUNT(*) AS priceCount 
                                    FROM 
                                      [dbo].[price]
                                    WHERE 
                                      [timestamp] >= DATEADD(day, -1, GETDATE()) 
                                    GROUP BY 
                                      [productId]
						) [prices]
						JOIN [dbo].[product] [p] on [p].[id] = [prices].[productId]
						JOIN [dbo].[price] [pr] on [pr].[productId] = [p].[id]
						LEFT JOIN [dbo].[category] [c] ON [p].[categoryId] = [c].[id]
						LEFT JOIN [dbo].[store] [s] ON [p].[storeId] = [s].[id]
						WHERE [prices].[priceCount] = 1 
						    AND [timestamp] >= DATEADD(day,-1,GETDATE())
						    AND [s].[hidden] = 0						
						GROUP BY [p].[id], [p].[articleNumber], [p].[name], [p].[brand], [c].[id], [pr].[timestamp], [s].[id], [c].[name], [s].[name], [pr].[value]
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