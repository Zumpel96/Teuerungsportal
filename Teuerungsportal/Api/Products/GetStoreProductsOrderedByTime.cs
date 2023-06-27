using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api.v2;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetStoreProductsOrderedByTime
{
    [FunctionName("GetStoreProductsOrderedByTimeV2")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v2/prices/store/{storeId}/{page}/time")] HttpRequest req,
        [Sql(
                commandText: @"
                          WITH [previous_prices] AS (
                              SELECT 
                                [p].[id], 
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
                                Lag([p].[value]) OVER (
                                  partition BY [p].[productId] 
                                  ORDER BY 
                                    [p].[timestamp]
                                ) AS [previousValue] 
                              FROM 
                                [dbo].[price] [p] 
                                JOIN [dbo].[product] [pr] ON [p].[productId] = [pr].[id] 
                                JOIN [dbo].[store] [s] ON [pr].[storeId] = [s].[id] 
                                LEFT JOIN [dbo].[category] [c] ON [pr].[categoryId] = [c].[id] 
                              WHERE 
                                [s].[hidden] = 0
                            ) 
                            SELECT 
                              [productId], 
                              [articleNumber],
                              [productName], 
                              [brand], 
                              [categoryId], 
                              [categoryName],
                              [storeId],
                              [storeName],
                              [timestamp],
							  [currentValue],
							  [previousValue]
                            FROM 
                              [previous_prices] [p]
							  JOIN [dbo].[store] [s] ON [s].[id] = [p].[storeId]
                            WHERE [s].[hidden] = 0 
                              AND [s].[id] = @storeId
                            ORDER BY
                              [timestamp] DESC
						    OFFSET ((@page - 1) * 10) ROWS FETCH NEXT 10 ROWS ONLY;",
                parameters: "@page={page},@storeId={storeId}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<PriceDbo> prices)
    {
        return new OkObjectResult(prices.Select(dbo => new Price(dbo)));
    }
}