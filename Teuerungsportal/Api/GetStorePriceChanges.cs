using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System;
using System.Collections.Generic;

public static class GetStorePriceChanges
{
    public class PriceDto
    {
        public Guid ProductId { get; set; }
        
        public double CurrentValue { get; set; }
        
        public double? PreviousValue { get; set; }

        public DateTime TimeStamp { get; set; }
    }

    [FunctionName("GetPriceChangesForStore")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "store/{storeId}/prices")] HttpRequest req,
        [Sql(
                commandText: @"WITH previous_prices AS (
                                  SELECT
                                    p.id,
		                            p.productId,
		                            pr.storeId,
                                    p.value,
                                    p.timestamp,
                                    LAG(p.value) OVER (PARTITION BY p.productId ORDER BY p.timestamp) AS previousValue
                                  FROM [dbo].[price] p
	                              JOIN [dbo].[product] pr ON p.productId = pr.Id
                                )
                                SELECT
                                  id,
	                              productId,
	                              storeId,
                                  value AS currentValue,
                                  previousValue,
                                  timestamp
                                FROM
                                  previous_prices
                                WHERE storeId = @storeId
                                ORDER BY timestamp DESC;",
                parameters: "@storeId={storeId}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<PriceDto> prices)
    {
        return new OkObjectResult(prices);
    }
}